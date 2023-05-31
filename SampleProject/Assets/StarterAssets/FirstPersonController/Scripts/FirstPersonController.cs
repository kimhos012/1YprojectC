using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;

//------------custom-----------------------------------------------------//	
		[Tooltip("Walk speed of the charater in m/s")]
		public float WalkSpeed = 2.0f;
		[Tooltip("Crouch speed of the chrarterr in m/s")]
		public float CrouchSpeed = 3.0f;
        [Tooltip("Zooming speed of the chrarterr in m/s")]
        public float SubFireSpeed = 2.0f;
		//CrouchScale
		private float scaleY;



		[Tooltip("GAMDO")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			//custom----

            scaleY = GameObject.Find("PlayerCapsule").GetComponent<Transform>().localScale.y;

            currentBullets = bulletsPerMag;

			if(weaponName == null)
            {
				weaponName = "Pistol";
            }

			_input.walk = false;

			Pistol = GameObject.Find("MainWeapon").transform.GetChild(1).gameObject;
            Rifle = GameObject.Find("MainWeapon").transform.GetChild(0).gameObject;

        }

		private void Update()
		{
			//이곳으로 옮겨진 이유는 총기의 반동이 화면 90도를 넘어가버리는 문제를 막기 위해 위로 옮겼습니다.
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);


            JumpAndGravity();
			GroundedCheck();
			Move();

			//custom--
			//


			UseCrouch();        //앉기 기능

			SwapWeapon();		//무기변경



            if (_input.Reload)		//재장전
			{
				if(!Isreload)
				{
					StartCoroutine(ReloadAmmo());
					Debug.Log("pressed R");
				}
				else { return; }
            }

			if (_input.MainFire && !Isreload)		//Mouse1
            {
                Fire();		//발사장치
            }
			else if(fireTimer < fireRate)
			{
				fireTimer += Time.deltaTime;        //타이머 = 실제 시간
			}
			
			if (_input.SubFire)		//Mouse2
            {

            }

			GunName = weaponName;		//Save text
			Ammo = currentBullets;
			AmmoTotal = bulletsTotal;

        }

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				//_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : _input.walk ? WalkSpeed : _input.crouch ? CrouchSpeed : _input.SubFire ? SubFireSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;			
			if (lfAngle > 360f) lfAngle -= 360f;			
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		//------------------------------------------무기변경------------------------------------------
		GameObject Pistol;
		GameObject Rifle;

		void SwapWeapon()			//60%제작함
		{
			if(_input.MainSwap)				//1
			{
				weaponName = "Rifle";
				bulletsPerMag = 30;
				bulletsTotal = 90;
				fireRate = 0.08f;
				Pistol.SetActive(false);
                Rifle.SetActive(true);
            }
			else if (_input.SubSwap)		//2
            {
                weaponName = "Pistol";
                bulletsPerMag = 12;
                bulletsTotal = 36;
                fireRate = 0.2f;
                Pistol.SetActive(true);
                Rifle.SetActive(false);
            }
            else if (_input.KnifeSwap)		//3
            {

            }
            else if (_input.GamjaSwap)		//G
            {

            }
        }

		//-----------------------------------------------------------------------------------------------------------------------//
		//앉기 키를 사용시, Scale를 조정해 보여지는 시야와 플레이어 크기를 줄임
		int i = 1;		//앉기 시스템
		bool Ctoggle = false;		//1회용 토글

		private void UseCrouch()
        {
			if (_input.crouch && Ctoggle == false)
			{
						//앉기
				i = 1;
				StartCoroutine("CrouchOn");
				Ctoggle = true;
			}
			else if(!_input.crouch && Ctoggle)
			{
				
				
				StartCoroutine("CrouchOff");

				Ctoggle = false;
			}
        }
		
		IEnumerator CrouchOn()
        {
			this.transform.localScale = new Vector3(.8f, (scaleY), .8f);
			i = 1;
			while (i < 30)
			{
				this.transform.localScale += new Vector3(0f, -0.01f, 0f);

				i++;
				yield return new WaitForSecondsRealtime(.005f);
			}
			if (!_input.crouch)
				StartCoroutine("CrouchOff");
		}

		IEnumerator CrouchOff()
		{
			
			this.transform.localScale = new Vector3(.8f, 0.71f, .8f);
			i = 1;
			while (i < 30)
			{
				this.transform.localScale += new Vector3(0f, 0.01f, 0f);

				i++;
				yield return new WaitForSecondsRealtime(.005f);
			}
			this.transform.localScale = new Vector3(0.8f, (scaleY), 0.8f);
		}

		//------------------------------------------------------------------------------------------------------------------------------------//

		//무기 세부 설정

		[Header("GUN")]
		[Tooltip("무기의 이름")]
        public string weaponName;       //1
		
		public static string GunName;		//static

        [Tooltip("탄창당 탄 개수")]
        public int bulletsPerMag;

        [Tooltip("총 총알")]
        public int bulletsTotal;				//2

		public static int AmmoTotal;		//static

        [Tooltip("장전된 총알 수")]
        public int currentBullets;          //3
		
		public static int Ammo;				//static

        [Tooltip("사거리")]
        public float range;
        [Tooltip("발사 간격")]
        public float fireRate;

		// 발사 속도 제어장치
		[Tooltip("발사 속도")]
        private float fireTimer;

		// 방향 설정
		[Tooltip("RayCast의 시작점과 방향")]
        public Transform shootPoint;

		//소리 설정

		[Header("Weapon use Sound")]
		[Tooltip("권총 쏘는소리")]
		public AudioSource PistolShoot;
        [Tooltip("소총 쏘는소리")]
        public AudioSource RifleShoot;
        [Tooltip("칼 쓰는소리")]
        public AudioSource KnifeShoot;

        [Header("Weapon Reload")]
        [Tooltip("권총 재장전 소리")]
        public AudioSource PistolReload;
        [Tooltip("소총 재장전 소리")]
        public AudioSource RifleReload;




        //------------------------------------------총알 발사 및 장전-------------------------------------
        public void Fire()			//투사체는 최적화가 어려워서 Raycast로 진행
        {
			if (fireTimer < fireRate && currentBullets >= 0)        //발사속도보다 빠르게 누르거나, 총알을 전부 사용시, return으로 돌려보냄
			{
				return;
			}
			else if (currentBullets <= 0)		//총알이 없을 때
			{
				Debug.Log("out of ammo");
				if(bulletsTotal <= 0 && currentBullets <= 0)		//총알 고갈
				{
					return;
				}
				else
				{
					StartCoroutine(ReloadAmmo());
				}
			}
			else if (fireTimer >= fireRate && currentBullets >= 0)  //발사
			{
				GunType();
				return;
			}
        }

        private void Raycasting()
        {
            Debug.Log("Fire!");
            RaycastHit hitinfo;
            if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hitinfo, range))     //맞음 확인
            {
                Debug.Log("Hit!");
            }
            currentBullets--;
            fireTimer = 0.0f;
        }


        //-------------------------------------발사 시 총 타입
        void GunType()
		{
			//반동
			switch(weaponName)
            {
				case "Pistol":
					StartCoroutine(PistolRecoil());
                    break;

				case "Rifle":
                    StartCoroutine(RifleRecoil());
					break;

				case "Knife":


					break;

				case null :
					break;
			}
		}
		//1시간정도 뜯어보니까 저게 수직이더라고요.
        IEnumerator PistolRecoil()
		{
			//shoot
			Raycasting();
            PistolShoot.Play();


            //recoil
            for (float i =0;i<8;i++)		//반동(제곱으로 순차적으로 올라가는 반동을 구현)
			{
				_cinemachineTargetPitch -= 0.5f*(Mathf.Abs(-i + 5));
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler((_cinemachineTargetPitch), 0.0f, 0.0f);      //카메라 수직
				yield return new WaitForSecondsRealtime(.01f);
            }
			yield return new WaitForSecondsRealtime(.1f);

            //end recoil

            for (float i = 0; i < 40; i++)       //반동(제곱으로 순차적으로 올라가는 반동을 구현)
            {
                _cinemachineTargetPitch += 0.2f;
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler((_cinemachineTargetPitch), 0.0f, 0.0f);      //카메라 수직
                yield return new WaitForSecondsRealtime(.01f);
            }
        }
        IEnumerator RifleRecoil()
        {
			if(currentBullets >= 1)
			{
				while (_input.MainFire && currentBullets >= 1)     //쏘는중
				{
					//shoot
					Raycasting();
					RifleShoot.Play();


					for (float i = 0; i < 8; i++)       //반동(제곱으로 순차적으로 올라가는 반동을 구현)
					{
						//recoil
						_cinemachineTargetPitch -= 0.08f * (Mathf.Abs(-i + 5));
						CinemachineCameraTarget.transform.localRotation = Quaternion.Euler((_cinemachineTargetPitch), 0.0f, 0.0f);      //카메라 수직
						yield return new WaitForSecondsRealtime(.005f);
					}
				}
				
				for (float i = 0; i < 40; i++)       //반동(제곱으로 순차적으로 올라가는 반동을 구현)
				{
					_cinemachineTargetPitch += 0.04f;
					CinemachineCameraTarget.transform.localRotation = Quaternion.Euler((_cinemachineTargetPitch), 0.0f, 0.0f);      //카메라 수직
					yield return new WaitForSecondsRealtime(.005f);
				}
                if (currentBullets >= 1) { yield break; }
            }
            yield break;
        }

		//------------------------------------------재장전---------------------------------------------
		bool Isreload;
		IEnumerator ReloadAmmo()
		{
			if (!Isreload)
			{
				Isreload = true;
				ReloadBar.bar = 1;
				while (ReloadBar.bar > 0)       //재장전 바
				{
					ReloadBar.bar -= 0.05f;
					yield return new WaitForSecondsRealtime(.1f);
				}

				//----------------재장전--------------------	
				Debug.Log("RELOAD");

				if (bulletsTotal < bulletsPerMag)       //총 총알 < 기본 장탄수
				{
					if(bulletsTotal + currentBullets < 16)
					{
                        currentBullets += bulletsTotal;
                        bulletsTotal = 0;
                    }
					else
					{
                        bulletsTotal = (bulletsPerMag - bulletsTotal);
                        currentBullets = bulletsPerMag;
                    }


				}
				else
				{
					bulletsTotal -= bulletsPerMag - currentBullets;
					currentBullets = bulletsPerMag;
				}
				Isreload = false;       //발사 잠금 해제
			}
			yield return null;
		}

        //-----------------------------------------상호작용---------------------------------------------

        public void Interaction()
        {
            if (_input.Interaction)
            {

            }
            else if (_input.SubInter)
            {

            }
        }
    }
}