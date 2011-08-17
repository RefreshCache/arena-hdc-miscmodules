using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Arena.Core.Communications;

namespace Arena.Custom.HDC.MiscModules.Communications
{
    [Description("Agent | SG Attendance Reminder")]
    public class AttendanceReminder : CommunicationType
    {
        public AttendanceReminder()
        {
        }


        public override string[] GetMergeFields()
        {
            List<string> fields = new List<string>();


            base.AddPersonMergeFields(fields);
            fields.Sort();

            return fields.ToArray();
        }
    }
}
