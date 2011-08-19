using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Arena.Core;
using Arena.Core.Communications;
using Arena.SmallGroup;

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


            fields.Add("##FirstName##");
            fields.Add("##FormalName##");
            fields.Add("##FullName##");
            fields.Add("##LastName##");
            fields.Add("##MiddleName##");
            fields.Add("##NickName##");
            fields.Add("##Suffix##");
            fields.Add("##Title##");
            fields.Add("##GroupName##");
            fields.Add("##GroupID##");

            fields.Sort();

            return fields.ToArray();
        }


        public void LoadFields(Dictionary<string, string> fields, Person person, Group group)
        {
            fields.Add("##FirstName##", person.FirstName);
            fields.Add("##FormalName##", person.FormalName);
            fields.Add("##FullName##", person.FullName);
            fields.Add("##LastName##", person.LastName);
            fields.Add("##MiddleName##", person.MiddleName);
            fields.Add("##NickName##", person.NickName);
            fields.Add("##Suffix##", person.Suffix.Value);
            fields.Add("##Title##", person.Title.Value);
            fields.Add("##GroupName##", group.Name);
            fields.Add("##GroupID##", group.GroupID.ToString());
        }
    }
}
