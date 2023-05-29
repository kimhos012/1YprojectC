using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		//
		public bool walk;
		public bool crouch;
		public bool MainFire;
        public bool SubFire;
        public bool Reload;
        public bool Interaction;


        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		//
		public void OnWalk(InputValue value)
        {
			WalkInput(value.isPressed);
        }
		public void OnCrouch(InputValue value)
        {
			CrouchInput(value.isPressed);
        }

		//마우스 1 2
		public void OnFire(InputValue value)
		{
			FireInput(value.isPressed);
		}
		public void OnSubFire(InputValue value)
		{
			SubFireInput(value.isPressed);
		}

		//재장전
		public void OnReload(InputValue value)
		{
			ReloadInput(value.isPressed);
		}
		//상호작용
		public void OnInteraction(InputValue value)
		{
			InteractionInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		//
		public void WalkInput(bool newWalkState)
		{
			walk = newWalkState;
		}

		public void CrouchInput(bool newCrouchState)
        {
			crouch = newCrouchState;
        }

		public void FireInput(bool newFireState)
		{
			MainFire = newFireState;
		}
        public void SubFireInput(bool newSubFireState)
        {
            SubFire = newSubFireState;
        }


        public void ReloadInput(bool newReloadState)
        {
             Reload = newReloadState;
        }
        public void InteractionInput(bool newInteractionState)
        {
            Interaction = newInteractionState;
        }



        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}