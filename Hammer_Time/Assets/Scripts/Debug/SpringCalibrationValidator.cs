using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quick validation tool for spring calibration
/// Attach to an empty GameObject and run in Play mode
/// NOTE: This is a manual validation helper - you'll need to fill in expected velocities
/// from actual test shots to validate the calibration.
/// </summary>
public class SpringCalibrationValidator : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Test these pullback distances")]
    public float[] testDistances = { 1.5f, 2.0f, 2.5f, 3.0f };
    
    [Header("Expected Results (fill in from manual tests)")]
    [Tooltip("Expected actual velocities at each test distance (from [Rock Release] logs)")]
    public float[] expectedActualVelocities = { 0f, 11.61f, 0f, 0f }; // Fill these in from tests!
    
    [Header("Tolerance")]
    [Tooltip("Acceptable error percentage")]
    public float acceptableErrorPercent = 5f;
    
    [Header("Results")]
    [TextArea(10, 20)]
    public string validationResults = "";
    
    [Header("Manual Calculation Parameters")]
    [Tooltip("Current calibration factor from TrajectorySimulator.cs")]
    public float currentCalibrationFactor = 0.72f;
    
    [Tooltip("Spring frequency (should match SpringJoint2D)")]
    public float springFrequency = 1.5f;
    
    [Tooltip("Spring damping ratio (should match SpringJoint2D)")]
    public float springDampingRatio = 0.2f;
    
    /// <summary>
    /// Call this from Inspector or another script to run validation
    /// </summary>
    [ContextMenu("Run Validation")]
    public void RunValidation()
    {
        validationResults = "=== SPRING CALIBRATION VALIDATION ===\n\n";
        validationResults += System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n";
        
        bool allTestsPassed = true;
        
        for (int i = 0; i < testDistances.Length; i++)
        {
            float distance = testDistances[i];
            float expectedActual = i < expectedActualVelocities.Length ? expectedActualVelocities[i] : 0f;
            
            // Manually calculate predicted velocity (replicating TrajectorySimulator logic)
            float predictedVelocity = CalculatePredictedVelocity(distance);
            
            // Calculate error
            float error = 0f;
            bool testPassed = true;
            string status = "";
            
            if (expectedActual > 0f)
            {
                error = ((predictedVelocity - expectedActual) / expectedActual) * 100f;
                testPassed = Mathf.Abs(error) <= acceptableErrorPercent;
                status = testPassed ? "? PASS" : "? FAIL";
            }
            else
            {
                status = "?? NO DATA";
                testPassed = false;
            }
            
            if (!testPassed && expectedActual > 0f) allTestsPassed = false;
            
            // Format output
            validationResults += $"Test {i + 1}: {distance:F1} units pullback\n";
            validationResults += $"  Predicted: {predictedVelocity:F2} m/s\n";
            
            if (expectedActual > 0f)
            {
                validationResults += $"  Expected:  {expectedActual:F2} m/s\n";
                validationResults += $"  Error:     {error:F1}% {(error > 0 ? "(over)" : "(under)")}\n";
                validationResults += $"  Status:    {status}\n";
            }
            else
            {
                validationResults += $"  Expected:  [Not set - run manual test and fill in]\n";
                validationResults += $"  Status:    {status}\n";
            }
            
            validationResults += "\n";
        }
        
        validationResults += "???????????????????????????????????\n";
        
        int testsWithData = 0;
        for (int i = 0; i < expectedActualVelocities.Length && i < testDistances.Length; i++)
        {
            if (expectedActualVelocities[i] > 0f) testsWithData++;
        }
        
        if (testsWithData == 0)
        {
            validationResults += "?? NO TEST DATA\n";
            validationResults += "Fill in expectedActualVelocities from manual tests!\n";
        }
        else
        {
            validationResults += allTestsPassed ? "? ALL TESTS PASSED\n" : "? SOME TESTS FAILED\n";
        }
        
        validationResults += "???????????????????????????????????\n\n";
        
        // Recommendations
        if (testsWithData > 0 && !allTestsPassed)
        {
            validationResults += "RECOMMENDATIONS:\n\n";
            
            // Check if error is consistent
            List<float> errors = new List<float>();
            for (int i = 0; i < testDistances.Length; i++)
            {
                if (i < expectedActualVelocities.Length && expectedActualVelocities[i] > 0f)
                {
                    float predictedVelocity = CalculatePredictedVelocity(testDistances[i]);
                    float error = ((predictedVelocity - expectedActualVelocities[i]) / expectedActualVelocities[i]) * 100f;
                    errors.Add(error);
                }
            }
            
            if (errors.Count > 0)
            {
                float avgError = 0f;
                foreach (float e in errors) avgError += e;
                avgError /= errors.Count;
                
                float errorVariance = 0f;
                foreach (float e in errors) errorVariance += Mathf.Pow(e - avgError, 2f);
                errorVariance /= errors.Count;
                float errorStdDev = Mathf.Sqrt(errorVariance);
                
                validationResults += $"Average Error: {avgError:F1}%\n";
                validationResults += $"Error Std Dev: {errorStdDev:F1}%\n\n";
                
                if (errorStdDev < 3f)
                {
                    // Consistent error - suggest single calibration adjustment
                    validationResults += "? Consistent error detected!\n";
                    validationResults += "  Recommendation: Adjust single calibration factor\n\n";
                    
                    float multiplier = 1f - (avgError / 100f);
                    float suggestedCalib = currentCalibrationFactor * multiplier;
                    
                    validationResults += $"  Current calibration: {currentCalibrationFactor:F2}\n";
                    validationResults += $"  Suggested calibration: {suggestedCalib:F2}\n\n";
                    validationResults += "  In TrajectorySimulator.cs, change:\n";
                    validationResults += $"  float calibrationFactor = {suggestedCalib:F2}f;\n\n";
                }
                else
                {
                    // Inconsistent error - suggest distance-dependent
                    validationResults += "? Inconsistent error detected!\n";
                    validationResults += "  Recommendation: Use distance-dependent calibration\n\n";
                    validationResults += "  In TrajectorySimulator.cs:\n";
                    validationResults += "  1. Comment out single calibration factor\n";
                    validationResults += "  2. Uncomment distance-dependent block\n";
                    validationResults += "  3. Tune each tier based on test results\n\n";
                }
            }
        }
        
        Debug.Log(validationResults);
    }
    
    /// <summary>
    /// Replicates TrajectorySimulator.CalculateInitialVelocityFromSpring logic
    /// </summary>
    private float CalculatePredictedVelocity(float springDistance)
    {
        // Physics calculation (same as TrajectorySimulator)
        float angularFrequency = 2f * Mathf.PI * springFrequency;
        float rockMass = 19.96f;
        float theoreticalVelocity = springDistance * angularFrequency;
        float dampingFactor = Mathf.Exp(-springDampingRatio * angularFrequency * 0.1f);
        float velocity = theoreticalVelocity * dampingFactor * currentCalibrationFactor;
        
        return velocity;
    }
}
