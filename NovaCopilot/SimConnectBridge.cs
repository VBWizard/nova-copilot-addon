using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Runtime.InteropServices;

public partial class SimConnectBridge
{
    private SimConnect _simConnect;
    private IntPtr _handle;
    private const int WM_USER_SIMCONNECT = 0x0402;

    public event Action<double> OnAltitudeReceived;
    public event Action<double> OnHeadingReceived;

    public SimConnectBridge(IntPtr hWnd)
    {
        _handle = hWnd;
    }

    public void Connect()
    {
        if (_simConnect == null)
        {
            try
            {
                _simConnect = new SimConnect("Nova Copilot", _handle, WM_USER_SIMCONNECT, null, 0);
                _simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
                _simConnect.AddToDataDefinition(DEFINITION.ID, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                _simConnect.RegisterDataDefineStruct<AltitudeStruct>(DEFINITION.ID);
                _simConnect.AddToDataDefinition(DEFINITION.HEADING, "PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                _simConnect.RegisterDataDefineStruct<HeadingStruct>(DEFINITION.HEADING);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Nova] Failed to connect SimConnect: " + ex.Message);
            }
        }
    }

    public void ExecuteCommand(NovaCopilotAddon.IntentResult result)
    {
        if (_simConnect == null || result == null || string.IsNullOrEmpty(result.SimEvent))
            return;

        if (result.SimEvent == "GET_ALTITUDE")
        {
            _simConnect.RequestDataOnSimObject(
                REQUEST.ALTITUDE,
                DEFINITION.ID,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                0, 0, 0);
            return;
        }

        if (result.SimEvent == "SYNC_HEADING")
        {
            _simConnect.RequestDataOnSimObject(
                REQUEST.HEADING,
                DEFINITION.HEADING,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                0, 0, 0);
            return;
        }

        uint eventID = 0;
        try
        {
            _simConnect.MapClientEventToSimEvent((EVENT_ID)eventID, result.SimEvent);
            _simConnect.TransmitClientEvent(
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                (EVENT_ID)eventID,
                (uint)(result.Value ?? 0),
                GROUP_ID.FLAG,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Nova] Failed to send command {result.SimEvent}: {ex.Message}");
        }
    }

    public void ReceiveMessage()
    {
        _simConnect?.ReceiveMessage();
    }

    private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        if ((REQUEST)data.dwRequestID == REQUEST.ALTITUDE)
        {
            var altitudeData = (AltitudeStruct)data.dwData[0];
            OnAltitudeReceived?.Invoke(altitudeData.Altitude);
        }
        else if ((REQUEST)data.dwRequestID == REQUEST.HEADING)
        {
            var headingData = (HeadingStruct)data.dwData[0];
            _simConnect.MapClientEventToSimEvent(EVENT_ID.HEADING_BUG_SET, "HEADING_BUG_SET");
            _simConnect.TransmitClientEvent(
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                EVENT_ID.HEADING_BUG_SET,
                (uint)Math.Round(headingData.Heading),
                GROUP_ID.FLAG,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY
            );
            OnHeadingReceived?.Invoke(headingData.Heading);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct AltitudeStruct { public double Altitude; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct HeadingStruct { public double Heading; }

    enum DEFINITION { ID, HEADING }
    enum REQUEST { ALTITUDE, HEADING }
    enum EVENT_ID { EVENT_0, HEADING_BUG_SET }
    enum GROUP_ID { FLAG }
}
