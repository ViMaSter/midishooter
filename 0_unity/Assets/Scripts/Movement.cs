using UnityEngine;
using System;
using System.Collections;

using NAudio.Midi;

public enum InputChannel
{
    NONE = -1,
    Velocity = 0,
    Direction = 1
}

public class Movement : MonoBehaviour
{
    #region Midi
    MidiIn MidiInput;

    #region Input settings
    public int velocityChannel = -1;
    public int directionChannel = -1;
    #endregion

    #region Movement input values
    public float targetVelocity = 0.0f;
    public float targetDirection = 0.0f;
    #endregion

    #region Movement config values
    public Vector2 MovementRegion = new Vector2(-0.5f, 1f);
    public float MovementSpeed = 12.5f;
    public float DirectionLerp = 0.8f;
    #endregion
    #endregion

    public InputChannel settingChannel = InputChannel.NONE;

    #region Helper functions
    void PrepareMIDIMessage(MidiInMessageEventArgs e, out int Channel, out int Value)
    {
        Channel = -1;
        Value = -1;

        try
        {
            NAudio.Midi.ControlChangeEvent evt = (NAudio.Midi.ControlChangeEvent)e.MidiEvent;
            Channel = (int)evt.Controller;
            Value = evt.ControllerValue;
        }
        catch (Exception ex) { }

        try
        {
            NAudio.Midi.NoteEvent evt = (NAudio.Midi.NoteEvent)e.MidiEvent;
            Channel = evt.NoteNumber;
            Value = evt.Velocity > 0 ? 1 : 0;
        }
        catch (Exception ex) { }
    }
    #endregion

    #region Input mapping
    void Start()
    {
        for (int i = 0; i < MidiIn.NumberOfDevices; i++)
        {
            if (MidiIn.DeviceInfo(i).ProductName.Contains("Launch Control XL"))
            {
                MidiInput = new MidiIn(i);
                break;
            }
        }

        // Start midi driver and map functions to events
        MidiInput.Start();
        MidiInput.MessageReceived += OnMidiMessage;
        MidiInput.ErrorReceived += OnMidiError;
    }

    #region Update
    void OnMidiMessage(object sender, MidiInMessageEventArgs e)
    {
        int Channel = -1;
        int Value = -1;

        PrepareMIDIMessage(e, out Channel, out Value);

        Debug.Log(Channel + "|" + Value);

        if ((InputChannel)settingChannel != InputChannel.NONE)
        {
            if (Value != 0)
            {
                InputMapping(Channel, Value);
            }
        }
        else
        {
            if (Value == 1)
            {
                switch (Channel)
                {
                    case 91:
                        settingChannel = InputChannel.Direction;
                        break;
                    case 92:
                        settingChannel = InputChannel.Velocity;
                        break;
                }
            }
            else
            {
                GameObjectManipulations(Channel, Value);
            }
        }
    }

    void InputMapping(int Channel, int Value)
    {
        switch (settingChannel)
        {
            case InputChannel.Velocity:
                velocityChannel = Channel;
                break;
            case InputChannel.Direction:
                directionChannel = Channel;
                break;
        }

        settingChannel = InputChannel.NONE;
    }

    void GameObjectManipulations(int Channel, int Value)
    {
        if (Channel == velocityChannel)
        {
            targetVelocity = (float)Value / 64f;
            targetVelocity = Mathf.Clamp(targetVelocity, -1, 1);
            targetVelocity = Mathf.Lerp(MovementRegion[0], MovementRegion[1], targetVelocity);
        }
        else if (Channel == directionChannel)
        {
            targetDirection = Value - 64;
            targetDirection = (float)targetDirection / 36f;
            targetDirection *= 180f;
            targetDirection *= -1;
        }
    }
    #endregion

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetDirection), DirectionLerp);
        transform.position += MovementSpeed * (targetVelocity) * transform.TransformDirection(Vector3.up) * Time.deltaTime;
    }

    void OnMidiError(object sender, MidiInMessageEventArgs e)
    {
        throw new Exception("MIDI-ERROR!\n" + e.ToString());
    }
    #endregion
}
