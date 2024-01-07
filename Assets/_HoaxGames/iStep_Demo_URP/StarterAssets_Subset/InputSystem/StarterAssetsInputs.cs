using UnityEngine;
using UnityEngine.EventSystems;
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

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private void Start()
		{
			InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
		}

		public void OnMove(InputValue value)
		{
			if (Cursor.lockState == CursorLockMode.None && cursorLocked)
			{
				MoveInput(Vector2.zero);
				return;
			}

			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
			{
				if (Cursor.lockState == CursorLockMode.None && cursorLocked)
				{
					LookInput(Vector2.zero);
					return;
				}

				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (Cursor.lockState == CursorLockMode.None && cursorLocked)
			{
				JumpInput(false);
				return;
			}

			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			if (Cursor.lockState == CursorLockMode.None && cursorLocked)
			{
				SprintInput(false);
				return;
			}

			SprintInput(value.isPressed);
		}

		public void OnInteractPrimary(InputValue value)
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(0)) return;
			SetCursorState(cursorLocked);
		}

		public void OnInteractSecondary(InputValue value)
		{
			if (EventSystem.current && EventSystem.current.IsPointerOverGameObject(1)) return;
			SetCursorState(cursorLocked);
		}

		public void OnEscape(InputValue value)
		{
			if (value.isPressed)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					SetCursorState(false);
				}
				else
				{
					SetCursorState(cursorLocked);
				}
			}
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