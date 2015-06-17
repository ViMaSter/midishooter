using UnityEngine;
using System;
using System.Collections;

using NAudio.Midi;

public class MidiDebugger : MonoBehaviour {
    MidiIn midiIn;

	void Start () {
        for (int i = 0; i < MidiIn.NumberOfDevices; i++)
        {
            if (MidiIn.DeviceInfo(i).ProductName.Contains("Launch Control XL"))
            {
                midiIn = new MidiIn(i);
                break;
            }
        }

        midiIn.Start();
        midiIn.MessageReceived += midiIn_MessageReceived;
        midiIn.ErrorReceived += midiIn_ErrorReceived;
	}

    void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
    {
        throw new Exception(e.ToString());
    }

    void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
    {
        int id = -1;
        int value = -1;

        try
        {
            NAudio.Midi.ControlChangeEvent evt = (NAudio.Midi.ControlChangeEvent)e.MidiEvent;
            id = (int)evt.Controller;
            value = evt.ControllerValue;
        }
        catch (Exception ex) { }

        try
        {
            NAudio.Midi.NoteEvent evt = (NAudio.Midi.NoteEvent)e.MidiEvent;
            id = evt.NoteNumber;
            value = evt.Velocity > 0 ? 1 : 0;
        }
        catch (Exception ex) { }

        RawMessage = e.RawMessage.ToString();
        Timestamp = e.Timestamp.ToString();
        Event = e.MidiEvent.ToString();
        ID = id.ToString();
        Value = value.ToString();
    }

    string RawMessage = "";
    string Timestamp = "";
    string Event = "";
    string ID = "";
    string Value = "";

    void OnGUI()
    {
        GUI.Label(new Rect(10, 005, 500, 20), RawMessage);
        GUI.Label(new Rect(10, 035, 500, 20), Timestamp);
        GUI.Label(new Rect(10, 065, 500, 20), Event);
        GUI.Label(new Rect(10, 095, 500, 20), ID);
        GUI.Label(new Rect(10, 125, 500, 20), Value);
    }
}
