using System.Linq;
using Extensions;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LODSlice : MonoBehaviour {
    public SliceOptions sliceOptions;
    public CallbackOptions callbackOptions;

    /// <summary>
    /// The number of times this fragment has been re-sliced.
    /// </summary>
    int _currentSliceCount;

    /// <summary>
    /// Collector object that stores the produced fragments
    /// </summary>
    GameObject _fragmentRoot;

    /// <summary>
    /// Slices the attached mesh along the cut plane
    /// </summary>
    /// <param name="sliceNormalWorld">The cut plane normal vector in world coordinates.</param>
    /// <param name="sliceOriginWorld">The cut plane origin in world coordinates.</param>
    public void ComputeSlice(Vector3 sliceNormalWorld, Vector3 sliceOriginWorld) {
        if(!GetComponent<MeshFilter>()) { return; }
        if(!transform.TryGetComponentInParent(out Rigidbody parentRigidBody)) { return; }
        
        // Temporär einen Rigidbody zum aktuellen Objekt hinzufügen
        var tempRigidbody = gameObject.AddComponent<Rigidbody>();
        tempRigidbody.mass = parentRigidBody.mass;
        tempRigidbody.linearVelocity = parentRigidBody.linearVelocity;
        tempRigidbody.angularVelocity = parentRigidBody.angularVelocity;
        tempRigidbody.linearDamping = parentRigidBody.linearDamping;
        tempRigidbody.angularDamping = parentRigidBody.angularDamping;
        tempRigidbody.useGravity = parentRigidBody.useGravity;
        tempRigidbody.isKinematic = parentRigidBody.isKinematic;
        // If the fragment root object has not yet been created, create it now
        if (_fragmentRoot == null) {
            // Create a game object to contain the fragments
            _fragmentRoot = new GameObject($"{name}Slices");
            _fragmentRoot.transform.SetParent(transform.parent);

            // Each fragment will handle its own scale
            _fragmentRoot.transform.position = transform.position;
            _fragmentRoot.transform.rotation = transform.rotation;
            _fragmentRoot.transform.localScale = Vector3.one;
        }

        var sliceTemplate = CreateSliceTemplate(parentRigidBody);
        var sliceNormalLocal = transform.InverseTransformDirection(sliceNormalWorld);
        var sliceOriginLocal = transform.InverseTransformPoint(sliceOriginWorld);

        Fragmenter.Slice(gameObject,
            sliceNormalLocal,
            sliceOriginLocal,
            sliceOptions,
            sliceTemplate,
            _fragmentRoot.transform);

        // Done with template, destroy it
        Destroy(sliceTemplate);

        // No need for a rigid body on the LOD object
        Destroy(parentRigidBody);
        
        // Add all new meshes to the LOD group and remove the old one
        if (transform.TryGetComponentInParent(out LODGroup lodGroup)) {
            var newMeshes = _fragmentRoot.GetComponentsInChildren<MeshFilter>();
            var newLodRenderers = new Renderer[newMeshes.Length];
    
            for (int i = 0; i < newMeshes.Length; i++) {
                newLodRenderers[i] = newMeshes[i].GetComponent<MeshRenderer>();
            }
            
            LOD[] existingLoDs = lodGroup.GetLODs();
            
            if (existingLoDs.Length > 0) {
                // Merge the new renderers with the existing ones
                var combinedRenderers = existingLoDs[0].renderers.Concat(newLodRenderers).ToArray();
                existingLoDs[0].renderers = combinedRenderers;
        
                // Set the new LODs
                lodGroup.SetLODs(existingLoDs);
                lodGroup.RecalculateBounds();
            }
        }
        
        // Fire the completion callback
        if (callbackOptions.onCompleted != null) {
            callbackOptions.onCompleted.Invoke();
        }
        
        // Deactivate the original object
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates a template object which each fragment will derive from
    /// </summary>
    /// <returns></returns>
    GameObject CreateSliceTemplate(Rigidbody parentRigidbody) {
        var obj = new GameObject {
            name = "Slice",
            tag = tag
        };

        // Update mesh to the new sliced mesh
        obj.AddComponent<MeshFilter>();

        // Add materials. Normal material goes in slot 1, cut material in slot 2
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new [] {
            GetComponent<MeshRenderer>().sharedMaterial,
            sliceOptions.insideMaterial
        };

        // Copy collider properties to fragment
        var thisCollider = GetComponent<Collider>();
        var fragmentCollider = obj.AddComponent<MeshCollider>();
        fragmentCollider.convex = true;
        fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        fragmentCollider.isTrigger = thisCollider.isTrigger;

        // Copy rigid body properties to fragment
        var fragmentRigidBody = obj.AddComponent<Rigidbody>();
        fragmentRigidBody.linearVelocity = parentRigidbody.linearVelocity;
        fragmentRigidBody.angularVelocity = parentRigidbody.angularVelocity;
        fragmentRigidBody.linearDamping = parentRigidbody.linearDamping;
        fragmentRigidBody.angularDamping = parentRigidbody.angularDamping;
            
        // Every fragment should have gravity and is not kinematic
        fragmentRigidBody.isKinematic = false;
        fragmentRigidBody.useGravity = true;

        // If refracturing is enabled, create a copy of this component and add it to the template fragment object
        if (sliceOptions.enableReslicing && _currentSliceCount < sliceOptions.maxResliceCount) {
            CopySliceComponent(obj);
        }

        return obj;
    }

    /// <summary>
    /// Convenience method for copying this component to another component
    /// </summary>
    /// <param name="obj">The GameObject to copy this component to</param>
    void CopySliceComponent(GameObject obj) {
        var sliceComponent = obj.AddComponent<LODSlice>();

        sliceComponent.sliceOptions = sliceOptions;
        sliceComponent.callbackOptions = callbackOptions;
        sliceComponent._currentSliceCount = _currentSliceCount + 1;
        sliceComponent._fragmentRoot = _fragmentRoot;
    }
}
