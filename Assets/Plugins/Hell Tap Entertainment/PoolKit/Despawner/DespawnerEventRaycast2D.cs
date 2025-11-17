//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	DespawnerEventRaycast2D.cs
//	Specific helper component for the Despawner
//
//	PoolKit For Unity, Created By Melli Georgiou
//	© 2019 Hell Tap Entertainment LTD
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellTap.PoolKit {

	[DisallowMultipleComponent]
	public class DespawnerEventRaycast2D : DespawnerEvent {

		// HELPERS
		// The size of the array determines how many raycasts will occur
		internal RaycastHit2D[] _raycastHitResults = new RaycastHit2D[30];
		int _numberOfRaycastHits = 0;

		// Create the ContactFilter2D helper
		#if UNITY_2023_1_OR_NEWER
			ContactFilter2D _contactFilter = new ContactFilter2D();
		#endif

		// UPDATE
		void Update(){
			
			// Make sure we can track collisions in this method ...
			if( despawner != null && despawner.despawnMode == Despawner.DespawnMode.AfterPhysicsRaycast2DEvent ){

				#if UNITY_EDITOR
					// If the user changes the raycast hit array length, update it in real-time, but only in the Editor.
					if( _raycastHitResults.Length != despawner.raycast2DmaxHits ){ 
						_raycastHitResults = new RaycastHit2D[despawner.raycast2DmaxHits];
					}
				#endif

				// Physics2D.RaycastNonAlloc is now marked as deprecated
				#if UNITY_2023_1_OR_NEWER

					// Create and configure the ContactFilter2D
					_contactFilter.SetLayerMask(despawner.filterLayers);
					_contactFilter.SetDepth(despawner.raycast2DMinZDepth, despawner.raycast2DMaxZDepth);

					// Use the new Physics2D.Raycast method
					_numberOfRaycastHits = Physics2D.Raycast(
					    transform.position, 
					    despawner.raycast2DDirection.normalized, 
					    _contactFilter, 
					    _raycastHitResults, 
					    despawner.raycast2DDistance
					);

				#else

					// Raycast from this object
					_numberOfRaycastHits = Physics2D.RaycastNonAlloc ( 
						
						transform.position, 						// Start position of the raycast
						despawner.raycast2DDirection.normalized, 	// Direction of the raycast (was Vector2.right)
						_raycastHitResults, 						// Size of raycast array
						despawner.raycast2DDistance, 				// Length of raycast (was Mathf.Infinity)
						despawner.filterLayers,						// Layermask
						despawner.raycast2DMinZDepth,				// Minimum Z depth, was: -Mathf.Infinity, 
						despawner.raycast2DMaxZDepth 				// Maximum Z depth, was: Mathf.Infinity, 
					);

				#endif				

				// Loop through the objects we hit and find the first validly hit object
				for( int i = 0; i < _numberOfRaycastHits; i++ ){
					
					// Make sure the hit object has a collider (to access the gameObject) and that it matches our layers and tags 
					if (	_raycastHitResults[i].collider != null &&
							despawner.CheckLayersAndTags( _raycastHitResults[i].collider.gameObject )
					){
		
						// Setup the despawner values and despawn now
						despawner.lastCollisionPoint = _raycastHitResults[i].point;
						despawner.lastCollidedPhysicsGameObject = _raycastHitResults[i].collider.gameObject;
						despawner.Despawn(true);

						//#if UNITY_EDITOR
						//	Debug.DrawLine(transform.position, _raycastHitResults[i].point, Color.green, 0.5f, true);
						//#endif

						// End now
						return;
					}
				}
				
			}
		}
	}
}