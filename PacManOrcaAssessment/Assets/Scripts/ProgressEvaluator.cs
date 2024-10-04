using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.WSA;

public class ProgressEvaluator : MonoBehaviour
{
    private enum GradeBand { Deactivate, Pass50, Pass60, Credit, Distinction, HD90, HD100 };
    private Action[] evalMethods;
    [SerializeField] private uint studentNumber = 0;
    [SerializeField] private GradeBand bandReached = GradeBand.Deactivate;
    //[SerializeField] private bool showSuccessMessages = false;

    private String lastLog;

    // Start is called before the first frame update
    void Start()
    {
        evalMethods = new Action[] { () => Pass50Band(), () => Pass60Band(), () => CreditBand(), () => DistinctionBand(), () => HighDist90Band(), () => HighDist100Band() };
        GradeBand loadedBand = EvalReader();
        //Debug.LogWarning("LOADED BAND == " + loadedBand);

        int i = 0;
        try
        {
            while (i < (int)bandReached)
            {
                if (i == 0)
                    CheckStudentNum();
                GradeBand currentBand = (GradeBand)i + 1;
                evalMethods[i]();
                if (currentBand <= GradeBand.HD90)
                {
                    EvalPassMessage(currentBand.ToString());
                    if (currentBand > loadedBand)
                        EvalOutput(currentBand);
                }
                else
                    EvalInProgressMessage(currentBand.ToString());
                i++;
            }
        }
        catch (EvalFailedException e)
        {
            EvalFailMessage(((GradeBand)i + 1).ToString(), e.Message);
        }
        catch (Exception e)
        {
            EvalFailMessage(((GradeBand)i + 1).ToString(), "An unknown error occured during the progress evaluation. It is likely " +
                "that you have made a mistake that the ProgressEvaluator system has not been setup to handle. Go back over the " +
                "steps for this grade band and see if you can spot anything wrong or ask your tutor. The full error is below for reference: ");
            Debug.LogError(e.GetType() + ": " + e.Message);
            Debug.LogError(e.GetType() + ": Full Stack Trace: " + e.StackTrace);
        }
    }


    private void Pass50Band()
    {
        // No tests. Check sprite layout
        Debug.LogWarning("PROGRESS EVALUATOR: Pass 50 Band: No automated tests for this band. " +
            "Please visually compare your sprite with the 5 frames shown on the left side of Status-50Percent.jpg -------------");
    }

    private void Pass60Band()
    {
        //Test: Animation import
        if (!Directory.Exists("Assets/Sprites"))
            throw new EvalFailedException("No folder named 'Sprites' found in your Project Window");
        if (!File.Exists("Assets/Sprites/BoxPinch.png"))
            throw new EvalFailedException("No asset named 'BoxPinch.png' found in your Project Window");

        TextureImporter ti = (TextureImporter)UnityEditor.TextureImporter.GetAtPath("Assets/Sprites/BoxPinch.png");
        if (ti == null)
            throw new EvalFailedException("Couldn't access the TextureImporter of BoxPinch animation asset. " +
                "You shouldn't be seeing this message, let William Raffe know.");
        if (ti.textureType != TextureImporterType.Sprite)
            throw new EvalFailedException("BoxPinch must be set as a Sprite type in the import settings");
        if (ti.spriteImportMode != SpriteImportMode.Multiple)
            throw new EvalFailedException("The Sprite Mode of BoxPinch must be set to Multiple");

        //Test: Visual Check    
        Debug.LogWarning("PROGRESS EVALUATOR: Pass 60 Band: Automated tests passed." +
           "Please visually compare your sprite slicing against Status-60Percent-1.jpg and Status-60Percent-3.jpg-------------");
    }

    private void CreditBand()
    {
        //Test: Camera settings
        if (!Camera.main.orthographic)
            throw new EvalFailedException("Camera is not set to orthograpic mode");
        if (!Mathf.Approximately(Camera.main.orthographicSize, 1.0f) && !Mathf.Approximately(Camera.main.orthographicSize, 0.5f))
            throw new EvalFailedException("Camera orthographic size is incorrect");

        //Test: Box
        GameObject box = GameObject.Find("Box");
        if (box == null)
            throw new EvalFailedException("No GameObject named 'Box' found in the scene");

        //Test: Check Animation folder
        if (!Directory.Exists("Assets/Animations"))
            throw new EvalFailedException("No folder named 'Animations' found in your Project Window");
        if (!File.Exists("Assets/Animations/PinchAnim.anim"))
            throw new EvalFailedException("No asset named 'PinchAnim' found in the Animations folder your Project Window");
        if (!File.Exists("Assets/Animations/SimpleAnimator.controller"))
            throw new EvalFailedException("No asset named 'SimpleAnimator' found in the Animations folder in your Project Window");
        if (!box.GetComponent<Animator>().runtimeAnimatorController.name.Equals("SimpleAnimator"))
            throw new EvalFailedException("Box does not have SimpleAnimator set as its Animator Controller");

        //Test: Animation settings
        AnimationClip anim = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/PinchAnim.anim");
        if (anim == null)
            throw new EvalFailedException("Couldn't access the AnimationClip of the PinchAnim animation asset. " +
                "You shouldn't be seeing this message, let William Raffe know.");
        if (!Mathf.Approximately(anim.frameRate, 10.0f))
            throw new EvalFailedException("The number of samples in a second for PincAnim must be 10 (i.e. 10 frames per second). " +
                "You currently have " + anim.frameRate + " samples.");
        if (!Mathf.Approximately(anim.length, 0.5f))
            throw new EvalFailedException("With 10fps and 5 frames of your animation, the length of the PinchAnim animation timeline" +
                " should be 0.5 seconds. It is currently " + anim.length + " seconds long.");
    }

    private void DistinctionBand()
    {
        // Test: Get animator
        AnimatorController ac = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/SimpleAnimator.controller");
        if (ac.layers.Length > 1)
            throw new EvalFailedException("The SimpleAnimator should only have 1 layer");

        // Test: SimpleAnimator number of states
        Animator boxAnimator = GameObject.Find("Box").GetComponent<Animator>();
        if (!boxAnimator.HasState(0, Animator.StringToHash("PinchAnim")))
            throw new EvalFailedException("SimpleAnimator should have two states: PinchAnim and Reverse");
        if (!boxAnimator.HasState(0, Animator.StringToHash("Reverse")))
            throw new EvalFailedException("SimpleAnimator should have two states: PinchAnim and Reverse");

        // Test: SimpleAnimator entry point
        AnimatorStateMachine sm = ac.layers[0].stateMachine;
        if (!sm.defaultState.name.Equals("PinchAnim"))
            throw new EvalFailedException("SimpleAnimator: The Entry node should be connected to the PinchAnim state");
        if (sm.defaultState.transitions.Length != 1)
            throw new EvalFailedException("SimpleAnimator: PinchAnim state should only have 1 outbound transition.");

        // Test: PinchAnim -> Reverse transition
        AnimatorStateTransition trans = sm.defaultState.transitions[0];
        if (!trans.hasExitTime)
            throw new EvalFailedException("SimpleAnimator: PinchAnim state should only transition out after a certain amount of time.");
        if (!Mathf.Approximately(trans.exitTime, 1.0f))
            throw new EvalFailedException("SimpleAnimator: PinchAnim state should exit after one full cycle of the state. " +
                "It currently exits at " + trans.exitTime * 100.0f + "% of the state.");
        if (trans.hasFixedDuration)
            throw new EvalFailedException("SimpleAnimator: It is highly recommended that you use untick Fixed Duration in the PinchAnim to Reverse transition settings. " +
                "This will help show the transition overlaps in a ratio of the state lengths, rather than raw seconds.");
        if (!Mathf.Approximately(trans.duration, 0.0f))
            throw new EvalFailedException("SimpleAnimator: PinchAnim state should transition instantly to the Reverse state " +
                "such that the PinchAnim exits and then Reverse starts immediatly. You are currently overlapping the " +
                "PinchAnim and Reverse states by " + trans.duration * 100 + "%  (hint: lookup Transition Duration)");
        if (!Mathf.Approximately(trans.offset, 0.0f))
            throw new EvalFailedException("SimpleAnimator: The Reverse state should start at the start of its animation when " +
                "transitioning from PinchAnim. You are currently starting the Reverse animation " + trans.offset * 100 + "% through the clip. (Hint: lookup Transition Offset)");

        // Test: Reverse -> PinchAnim transition
        if (!Mathf.Approximately(trans.destinationState.speed, -1.0f))
            throw new EvalFailedException("SimpleAnimator: The Reverse state should play the pinching animation backwards (hint: lookup the Speed property of Animation States)");
        if (trans.destinationState.transitions.Length < 1)
            throw new EvalFailedException("SimpleAnimator: PinchAnim state should have at least 1 outbound transition.");
        trans = FindTransitionByDest(trans.destinationState.transitions, "PinchAnim");
        if (trans == null)
            throw new EvalFailedException("SimpleAnimator: Couldn't find a transition from Reverse state to PinchAnim state");
        if (!Mathf.Approximately(trans.exitTime, 1.0f))
            throw new EvalFailedException("SimpleAnimator: Reverse state should exit after one full cycle of the state. " +
                "It currently exits at " + trans.exitTime * 100.0f + "% of the state.");
        if (trans.hasFixedDuration)
            throw new EvalFailedException("SimpleAnimator: It is highly recommended that you use untick Fixed Duration in the Reverse to PinchAnim transition settings. " +
                "This will help show the transition overlaps in a ratio of the state lengths, rather than raw seconds.");
        if (!Mathf.Approximately(trans.duration, 0.0f))
            throw new EvalFailedException("SimpleAnimator: Reverse state should transition instantly to the PinchAnim state " +
                "such that the Reverse state exits and then PinchAnim starts immediatly. You are currently overlapping the " +
                "Reverse and PinchAnim states by " + trans.duration * 100 + "%  (hint: lookup Transition Duration)");
        if (!Mathf.Approximately(trans.offset, 0.0f))
            throw new EvalFailedException("SimpleAnimator: The PinchAnim state should start at the start of its animation when " +
                "transitioning from PinchAnim. You are currently starting the Reverse animation " + trans.offset * 100 + "% through the clip. (Hint: lookup Transition Offset)");
    }


    private void HighDist90Band()
    {
        // Test: Rotate Animation
        if (Directory.GetFiles("Assets/Animations/").Length < 3)
            throw new EvalFailedException("The Animations folder should have 3 files in it: PinchAnim, SimpleAnimator, and a rotation animation clip. " +
                "You can also do the rotation animation via code instead of an animation clip, but the ProgressEvaluator won't check this, so use visual " +
                "checking with the Status-100Percent executables instead");

        // Test: RotateParam
        AnimatorController ac = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/SimpleAnimator.controller");
        if (ac.parameters.Length != 1)
            throw new EvalFailedException("SimpleAnimator: The animator should have only 1 parameter named RotateParam");
        if (!ac.parameters[0].name.Equals("RotateParam"))
            throw new EvalFailedException("SimpleAnimator: The animator should have only 1 parameter named RotateParam");
        if (ac.parameters[0].type != AnimatorControllerParameterType.Trigger)
            throw new EvalFailedException("SimpleAnimator: RotateParam's type is not optimal. Lookup the various types available (bool, int, float, trigger) and their purpose");

        // Test: Rotate state and transition conditions
        AnimatorState reverseState = FindStateByName(ac.layers[0].stateMachine.states, "Reverse");
        if (reverseState == null)
            throw new EvalFailedException("SimpleAnimator: State named Reverse could not be found");
        AnimatorStateTransition trans = FindTransitionByDest(reverseState.transitions, "Rotate");
        if (trans == null)
            throw new EvalFailedException("SimpleAnimator: Couldn't find a transition from Reverse state to Rotate state");
        if (!reverseState.transitions[0].destinationState.name.Equals("Rotate"))
            throw new EvalFailedException("SimpleAnimator: The priority order of the transitions leaving the reverse state is incorrect.");
        if (!trans.conditions[0].parameter.Equals("RotateParam"))
            throw new EvalFailedException("SimpleAnimator: RotateParam must be used as a transition condition on the Reverse -> Rotate transition.");

        // Test: Reverse -> Rotate transition settings
        if (!Mathf.Approximately(trans.exitTime, 1.0f))
            throw new EvalFailedException("SimpleAnimator: Reverse state should exit to Rotate state only after it has fineshed the current cycle of its animation.");
        if (trans.hasFixedDuration)
            throw new EvalFailedException("SimpleAnimator: It is highly recommended that you use untick Fixed Duration in the Reverse to Rotate transition settings. " +
                "This will help show the transition overlaps in a ratio of the state lengths, rather than raw seconds.");
        if (!Mathf.Approximately(trans.duration, 0.0f))
            throw new EvalFailedException("SimpleAnimator: Reverse state should transition instantly to the Rotate state " +
                "such that the Reverse state exits and then Rotate starts immediatly. You are currently overlapping the " +
                "Reverse and Rotate states by " + trans.duration * 100 + "%  (hint: lookup Transition Duration)");
        if (!Mathf.Approximately(trans.offset, 0.0f))
            throw new EvalFailedException("SimpleAnimator: The Rotate state should start at the start of its animation when " +
                "transitioning from Reverse. You are currently starting the Rotate animation " + trans.offset * 100 + "% through the clip. (Hint: lookup Transition Offset)");

        // Test: Rotate -> PinchAnim
        trans = FindTransitionByDest(trans.destinationState.transitions, "PinchAnim");
        if (trans == null)
            throw new EvalFailedException("SimpleAnimator: Couldn't find a transition from Rotate state to PinchAnim state");
        if (!Mathf.Approximately(trans.exitTime, 1.0f))
            throw new EvalFailedException("SimpleAnimator: Rotate state should exit to PinchAnim state only after it has fineshed the current cycle of its animation.");
        if (trans.hasFixedDuration)
            throw new EvalFailedException("SimpleAnimator: It is highly recommended that you use untick Fixed Duration in the Rotate to PinchAnim transition settings. " +
                "This will help show the transition overlaps in a ratio of the state lengths, rather than raw seconds.");
        if (!Mathf.Approximately(trans.duration, 0.0f))
            throw new EvalFailedException("SimpleAnimator: Rotate state should transition instantly to the PinchAnim state " +
                "such that the Rotate state exits and then PinchAnim starts immediatly. You are currently overlapping the " +
                "Rotate and PinchAnim states by " + trans.duration * 100 + "%  (hint: lookup Transition Duration)");
        if (!Mathf.Approximately(trans.offset, 0.0f))
            throw new EvalFailedException("SimpleAnimator: The PinchAnim state should start at the start of its animation when " +
                "transitioning from Rotate. You are currently starting the PinchAnim animation " + trans.offset * 100 + "% through the clip. (Hint: lookup Transition Offset)");

        // Test: Rotation Root Motion
        Animator boxAnimator = GameObject.Find("Box").GetComponent<Animator>();
        if (!boxAnimator.applyRootMotion)
            throw new EvalFailedException("SimpleAnimator: There is a good chance your pinching animation doesn't look correct after the box rotates. To fix this, lookup 'Root Motion'");
    }

    private void HighDist100Band()
    {
        // Test: Background Music setup
        GameObject audioGO = GameObject.Find("Background Music");
        if (audioGO == null)
            throw new EvalFailedException("No GameObject named Background Music was found");
        AudioSource sc = audioGO.GetComponent<AudioSource>();
        if (sc == null)
            throw new EvalFailedException("Background Music GameObject does not have an audio source component");
        if (!sc.clip.name.Equals("BackgroundMusic"))
            throw new EvalFailedException("Background Music does not have the correct audio clip assigned");
        if (sc.volume > 0.51f)
            throw new EvalFailedException("Background Music volume is incorrect");
        if (!sc.loop)
            throw new EvalFailedException("Background Music isn't looping.");

        // Test: Clicked sound setup
        audioGO = GameObject.Find("Clicked Sound");
        if (audioGO == null)
            throw new EvalFailedException("No GameObject named Clicked Sound was found");
        sc = audioGO.GetComponent<AudioSource>();
        if (sc == null)
            throw new EvalFailedException("Clicked Sound GameObject does not have an audio source component");
        if (!sc.clip.name.Equals("ButtonClicked"))
            throw new EvalFailedException("BClicked Sound does not have the correct audio clip assigned");
        if (!Mathf.Approximately(sc.volume, 1.0f))
            throw new EvalFailedException("Clicked Sound volume is incorrect");
        if (sc.loop)
            throw new EvalFailedException("Clicked Sound is looping.");

        StartCoroutine(HD100Coroutine());
    }

    private IEnumerator HD100Coroutine()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        yield return null;
        if (!GameObject.Find("Clicked Sound").GetComponent<AudioSource>().isPlaying)
            throw new EvalFailedException("The Clicked Sound wasn't played immediately after the Spacebar was pressed.");

        EvalPassMessage(GradeBand.HD100.ToString());
        EvalOutput(GradeBand.HD100);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        lastLog = logString;
    }

    private void CheckStudentNum()
    {
        if (studentNumber < 10000000)
            throw new EvalFailedException("Student Number Check: Invalid student number. This must be an 8 digit student ID.");
    }

    private static void EvalFailMessage(String band, String message)
    {
        Debug.LogError("PROGRESS EVALUATOR: " + band + ": " + message);
    }

    private static void EvalPassMessage(String band)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: " + band + " Band: No common mistakes found. -------------");
    }

    private static void EvalInProgressMessage(String band)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: " + band + " Band: This band is doing checks over multiple frames. Press keys identified in this band of the PDF to continue the checks -------------");
    }


    private void EvalOutput(GradeBand evalGrade)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: Remember to also check your progress against the Status-xPercent.png files and " +
            "Status-100Percent-Windows or Status-100Percent-Mac.app executables.");
        using BinaryWriter writer = new BinaryWriter(File.Open("ProjectSettings\\ProgEv", FileMode.Create));
        writer.Write(evalGrade.ToString());
        writer.Write(studentNumber);
        writer.Write(DateTime.Now.ToString("MM/dd/yyyy"));
        //writer.Write(DateTime.Now.Year.ToString());
    }

    private GradeBand EvalReader()
    {
        try
        {
            using BinaryReader reader = new BinaryReader(File.Open("ProgEv", FileMode.Open));
            return (GradeBand)Enum.Parse(typeof(GradeBand), reader.ReadString());
        }
        catch (Exception e)
        {
            lastLog = e.Message;
            return GradeBand.Deactivate;
        }
    }

    private bool VectApprox(Vector3 a, Vector3 b)
    {
        if (Vector3.Distance(a, b) < 0.01f)
            return true;
        else
            return false;
    }

    private AnimatorStateTransition FindTransitionByDest(AnimatorStateTransition[] transitions, String inName)
    {
        foreach (AnimatorStateTransition trans in transitions)
            if (trans.destinationState.name.Equals(inName))
                return trans;
        return null;
    }

    private AnimatorState FindStateByName(ChildAnimatorState[] states, String inName)
    {
        foreach (ChildAnimatorState state in states)
            if (state.state.name.Equals(inName))
                return state.state;
        return null;
    }
}

public class EvalFailedException : Exception
{
    public EvalFailedException(string message) : base(message) { }
}
