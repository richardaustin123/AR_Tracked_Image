using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour {
    // Reference to AR tracked imager manager component
    private ARTrackedImageManager _trackedImageManager;

    // List of prefeabs to instantiate - these should be named teh same 
    // as their corresponding 2D images in the reference image library
    public GameObject[] ArPrefabs;

    // Keep dictionary array of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void Awake() {
        // Cache a reference to AR tracked image manager component
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable() {
        // Attatch event handler when tracked images change
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable() {
        // Detatch event handler when tracked images change
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Event handler for tracked images changing
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        
        // Loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added) {
            // Get the name of the reference image 
            var imageName = trackedImage.referenceImage.name;
            // Now loop over  the array of prefabs
            foreach (var curPrefab in ArPrefabs) {
                // check whether this prefab matches the tracked image name, and that 
                // the prefab has not already been instantiated
                if (string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0 
                    && !_instantiatedPrefabs.ContainsKey(imageName)) {
                    // Instantiate prefab at the tracked image's position and rotation
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    // Add the new prefab to the dictionary array
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }

        // For all prefabs that have been created so far, set them active or not
        // depending on whether their corresponding image is being tracked
        foreach (var trackedImage in eventArgs.updated) {
            _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        // If the AR subsystem has gievn up looking for a tracked image
        foreach (var trackedImage in eventArgs.removed) {
            // Destroy the prefab
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            // Remove the prefab from the dictionary array
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            // Or, simply set the prefab instance to inactive
            //_instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }
}
