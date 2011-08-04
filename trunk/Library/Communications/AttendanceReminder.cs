using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arena.Core.Communications;

namespace Arena.Custom.HDC.MiscModules.Communications
{
    public class AttendanceReminder : CommunicationType
    {
        public override string[] GetMergeFields()
        {
            List<string> fields = new List<string>();


            base.AddPersonMergeFields(fields);
            fields.Sort();

            return fields.ToArray();
        }
    }
}
