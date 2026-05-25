public static class MobileTouchLock
{
    public static int JoystickTouchId = -1;
    public static int CameraTouchId = -1;
    public static bool IsZooming = false;

    public static bool HasJoystick => JoystickTouchId >= 0;
    public static bool HasCamera => CameraTouchId >= 0;

    public static void Reset()
    {
        JoystickTouchId = -1;
        CameraTouchId = -1;
        IsZooming = false;
    }
}