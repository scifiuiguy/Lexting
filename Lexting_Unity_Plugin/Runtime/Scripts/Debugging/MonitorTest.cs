using System.Runtime.InteropServices;

public class MonitorTest
{

    [DllImport("user32.dll")]
    private static extern void SaveFileDialog(); //in your case : OpenFileDialog

    private void myMethod()
    {
        //System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

        //your code
    }
}