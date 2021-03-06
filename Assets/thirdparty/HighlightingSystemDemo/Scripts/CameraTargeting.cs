using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraTargeting : MonoBehaviour
{
	public static CameraTargeting inst = null;
	// Which layers targeting ray must hit (-1 = everything)
	public LayerMask targetingLayerMask = -1;
	
	// Targeting ray length
	private float targetingRayLength = Mathf.Infinity;
	
	// Camera component reference
	private Camera cam;
	
	public Transform lasttargetTransform = null;
	
	float doubleClickStart = 0;
	
	void Awake()
	{
		inst = this;
		cam = GetComponent<Camera>();
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			if(lasttargetTransform != null)
			{
				HighlightableObject targetho = lasttargetTransform.GetComponent<HighlightableObject>();
				targetho.Off();
				
				if(game_ui_autopos.target_bar != null)
				{
					game_ui_autopos.hideTargetBar();
					TargetManger.cancelAutoAttack();
				}
			}
			return;
		}
		
		TargetingRaycast();
	}
	
	public void setTarget(Transform targetTransform)
	{
		if(lasttargetTransform != null)
		{
			HighlightableObject targetho = lasttargetTransform.GetComponent<HighlightableObject>();
			targetho.FlashingOff();
			lasttargetTransform = null;
		}
		
		if(targetTransform == null)
			return;
		
		HighlightableObject ho = targetTransform.GetComponent<HighlightableObject>();
		if(ho == null)
			return;
					
		lasttargetTransform = targetTransform;
		// Start flashing with frequency = 2
		ho.FlashingOn(2f);
		
		GameEntityCtrl ge = lasttargetTransform.gameObject.GetComponent<GameEntityCtrl>();
		TargetManger.setTarget(ge.seo);
	}
	
	public void TargetingRaycast()
	{
		// Current mouse position on screen
		Vector3 mp = Input.mousePosition;
		
		Transform targetTransform = null;
		
		// If camera component is available
		if (cam != null)
		{
			RaycastHit hitInfo;
			
			// Create a ray from mouse coords
			Ray ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));
			
			// Targeting raycast
			if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, targetingRayLength, targetingLayerMask.value))
			{
				if(RPG_Animation.instance != null && hitInfo.collider.transform.gameObject != RPG_Animation.instance.transform.gameObject)
				{
					// Cache what we've hit
					targetTransform = hitInfo.collider.transform;
				}
			}
		}
		
		// If we've hit an object during raycast
		if (targetTransform != null)
		{
			// And this object has HighlightableObject component
			HighlightableObject ho = targetTransform.GetComponent<HighlightableObject>();

			if (ho != null)
			{
				// If left mouse button down
				if (Input.GetButtonDown("Fire1"))
				{
					if(lasttargetTransform == targetTransform)
					{
						if ((Time.time - doubleClickStart) < 0.3f)
						{
							Common.DEBUG_MSG("Double Clicked!");
							setTarget(targetTransform);
						}
						
						doubleClickStart = Time.time;
						return;
					}
					
					doubleClickStart = Time.time;
					
					setTarget(targetTransform);
				}
				
				/*
				// If right mouse button is up
				if (Input.GetButtonUp("Fire2"))
					// Stop flashing
					ho.FlashingOff();
				
				// If middle mouse button is up
				if (Input.GetButtonUp("Fire3"))
					// Switch flashing
					ho.FlashingSwitch();
				*/
				
				// One-frame highlighting (to highlight an object which is currently under mouse cursor)
				ho.On(Color.red);
			}
		}
	}
}
