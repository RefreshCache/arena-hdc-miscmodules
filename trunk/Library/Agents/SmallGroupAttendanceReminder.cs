using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Arena.Core;
using Arena.Portal;
using Arena.SmallGroup;
using Agent;

using Arena.Custom.HDC.MiscModules.Communications;

namespace Arena.Custom.HDC.MiscModules.Agents
{
    [Serializable]
    [Description("Sends an e-mail to all small-group leaders who have not yet taken attendance for their small group.")]
    public class SmallGroupAttendanceReminder : AgentWorker
    {
        #region Private Properties

        private const int STATE_OK = 0;

        private int[] _clusterTypes = null;
        private String _clusterTypeIds = String.Empty;
        private int[] _topics = null;
        private String _topicIds = String.Empty;
        private int[] _assistantLeaderRoles = null;
        private String _assistantLeaderRoleIds = String.Empty;
        private int _categoryID = 1;
        private int _organizationID = 1;
        private int _gracePeriod = 1;
        private Boolean _debug = false;
        private StringBuilder _message = new StringBuilder();

        private int[] AssistantLeaderRoles
        {
            get
            {
                if (_assistantLeaderRoles == null)
                {
                    String[] items = _assistantLeaderRoleIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> list = new List<int>();

                    foreach (String item in items)
                    {
                        list.Add(Convert.ToInt32(item));
                    }

                    _assistantLeaderRoles = list.ToArray();
                }

                return _assistantLeaderRoles;
            }
        }
        private int[] ClusterTypes
        {
            get
            {
                if (_clusterTypes == null)
                {
                    String[] items = _clusterTypeIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> list = new List<int>();

                    foreach (String item in items)
                    {
                        list.Add(Convert.ToInt32(item));
                    }

                    _clusterTypes = list.ToArray();
                }

                return _clusterTypes;
            }
        }
        private int[] Topics
        {
            get
            {
                if (_topics == null)
                {
                    String[] items = _topicIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> list = new List<int>();

                    foreach (String item in items)
                    {
                        list.Add(Convert.ToInt32(item));
                    }

                    _topics = list.ToArray();
                }

                return _topics;
            }
        }

        #endregion


        #region Public Properties

        public String Message { get { return _message.ToString(); } }

        #endregion


        #region Agent Settings

        [TextSetting("Cluster Type IDs", "Comma separated list of cluster types to be included in the reminder e-mails. Leave blank for all in system.", false)]
        public String ClusterTypeIDs { get { return _clusterTypeIds; } set { _clusterTypeIds = value; } }

        [TextSetting("Topic IDs", "Comma separated list of small group topics to be included in the reminder e-mails. Leave blank for all in system.", false)]
        public String TopicIDs { get { return _topicIds; } set { _topicIds = value; } }

        [BooleanSetting("Assistant Leader Roles", "Small Group Leaders will always get an e-mail. If you wish assistant leaders to also get the e-mail enter the comma separated list of small group role ID numbers that you wish to also receive a reminder e-mail.", true, false)]
        public String AssistantLeaderRoleIDs { get { return _assistantLeaderRoleIds; } set { _assistantLeaderRoleIds = value; } }

        [NumericSetting("Category ID", "Small Group Category to work with when sending e-mail reminders.", true)]
        public int CategoryID { get { return _categoryID; } set { _categoryID = value; } }

        [NumericSetting("Organization ID", "Organization ID to work with for processing e-mail reminders.", true)]
        public int OrganizationID { get { return _organizationID; } set { _organizationID = value; } }

        [NumericSetting("Grace Period", "Number of 24-hour periods that must pass before a reminder e-mail is sent.", true)]
        public int GracePeriod { get { return _gracePeriod; } set { _gracePeriod = value; } }

        [BooleanSetting("Debug", "Enable debug mode. When Debug mode is on no e-mails will be sent but extra information will be available in the output.", true, true)]
        public Boolean Debug { get { return _debug; } set { _debug = value; } }

        #endregion


        #region AgentWorker Overrides

        /// <summary>
        /// This method is called by the AgentWorker process to cause this worker to begin
        /// it's work.
        /// </summary>
        /// <param name="previousWorkersActive">Indicates if another instance of this worker is still actively running.</param>
        /// <returns>A WorkerResult object which identifies the status of this run.</returns>
        public override WorkerResult Run(bool previousWorkersActive)
        {
            int state;
            string message;


            if (RunIfPreviousWorkersActive || !previousWorkersActive)
            {
                WorkerResultStatus status;


                try
                {
                    status = Process(out state);
                    message = Message;
                }
                catch (Exception e)
                {
                    status = WorkerResultStatus.Exception;
                    state = STATE_EXCEPTION;
                    message = e.Message;
                }

                return new WorkerResult(state, status, String.Format(Description), message);
            }
            else
                return new WorkerResult(STATE_OK, WorkerResultStatus.Ok, String.Format(Description), "Did not run because previous worker instance is still active.");
        }

        #endregion


        /// <summary>
        /// Process all small group cluster types defined for this category ID.
        /// </summary>
        /// <param name="state">Agent specific exit state.</param>
        /// <returns>A WorkerResultStatus value indicating if this agent worker finished processing successfully.</returns>
        WorkerResultStatus Process(out int state)
        {
            GroupClusterCollection clusters;
            String[] clusterTypes;

            
            state = STATE_OK;

            //
            // Prepare the list of valid cluster types and load the list of cluster types
            // for the default category.
            //
            clusterTypes = ClusterTypeIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            clusters = new SmallGroup.GroupClusterCollection(CategoryID, OrganizationID);

            //
            // Walk each cluster and process it.
            //
            foreach (GroupCluster cluster in clusters)
            {
                if (ProcessCluster(cluster) == false)
                    return WorkerResultStatus.Critical;
            }

            return WorkerResultStatus.Ok;
        }


        /// <summary>
        /// Process a single small group cluster. If the cluster has child-clusters then
        /// each child cluster is processed as well. If the cluster has small groups then
        /// pass along the small groups to the ProcessGroup method.
        /// </summary>
        /// <param name="cluster">The GroupCluster to be processed.</param>
        /// <returns>True/false status indicating if a fatal error occurred.</returns>
        Boolean ProcessCluster(GroupCluster cluster)
        {
            //
            // If they have limited the cluster types and this cluster type is not in
            // the list of valid cluster types, then skip it.
            //
            if (ClusterTypes.Length > 0 && ClusterTypes.Contains(cluster.ClusterTypeID) == false)
                return true;

            if (Debug)
                _message.AppendFormat("Processing cluster '{0}'\r\n", cluster.Name);

            //
            // Process each cluster level under this cluster.
            //
            foreach (GroupCluster child in cluster.ChildClusters)
            {
                if (ProcessCluster(child) == false)
                    return false;
            }

            //
            // Process each small group under this cluster.
            //
            foreach (Group group in cluster.SmallGroups)
            {
                if (ProcessGroup(group) == false)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Process a single small group. Determine if the small group leader has taken
        /// attendance for their last small group and if not send the leader (and
        /// optionally assistant-leaders) an e-mail to remind them.
        /// </summary>
        /// <param name="group">The small group to check attendance for.</param>
        /// <returns>True/false status indicating if a fatal error occurred.</returns>
        Boolean ProcessGroup(Group group)
        {
            GroupOccurrence occurrence = null;
            DayOfWeek   dow;
            DateTime    meetingDate;


            //
            // If this small group is not active, ignore it.
            //
            if (group.Active == false)
                return true;

            //
            // If they have limited the cluster types and this cluster type is not in
            // the list of valid cluster types, then skip it.
            //
            if (Topics.Length > 0 && Topics.Contains(group.Topic.LookupID) == false)
                return true;

            //
            // Determine the meeting day of the week.
            //
            if (group.MeetingDay == null)
                return true;
            if (group.MeetingDay.Value.Equals("Sunday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Sunday;
            else if (group.MeetingDay.Value.Equals("Monday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Monday;
            else if (group.MeetingDay.Value.Equals("Tuesday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Tuesday;
            else if (group.MeetingDay.Value.Equals("Wednesday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Wednesday;
            else if (group.MeetingDay.Value.Equals("Thursday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Thursday;
            else if (group.MeetingDay.Value.Equals("Friday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Friday;
            else if (group.MeetingDay.Value.Equals("Saturday", StringComparison.CurrentCultureIgnoreCase))
                dow = DayOfWeek.Saturday;
            else
            {
                if (Debug)
                    _message.AppendFormat("Could not determine meeting day of week for small group {0}, value was '{1}'\r\n", group.Name, group.MeetingDay.Value);

                return true;
            }
            
            //
            // Walk backwards from today. We don't consider "today" a valid day
            // so start on the previous day.
            //
            for (DateTime dt = DateTime.Now.AddDays(-1); ; dt = dt.AddDays(-1))
            {
                if (dt.DayOfWeek == dow)
                {
                    meetingDate = dt;
                    break;
                }
            }

            //
            // Find the existing occurrence for this group.
            //
            foreach (GroupOccurrence occ in group.Occurrences)
            {
                //
                // The occurrence must match the date portion of the expected meeting date.
                //
                if (occ.StartTime.Year == meetingDate.Year && occ.StartTime.Month == meetingDate.Month &&
                    occ.StartTime.Day == meetingDate.Day)
                {
                    occurrence = occ;
                    break;
                }
            }

            //
            // If the occurrence already has attendance taken, then they are all good.
            //
            if (occurrence != null && occurrence.Attendance > 0)
                return true;

            //
            // Make sure the grace period has passed.
            //
            meetingDate = new DateTime(meetingDate.Year, meetingDate.Month, meetingDate.Day, 23, 59, 59);
            if (meetingDate.CompareTo(DateTime.Now.AddDays(-(GracePeriod + 1))) > 0 &&
                meetingDate.CompareTo(DateTime.Now.AddDays(-GracePeriod)) < 0)
            {
                List<Person> leaders = new List<Person>();

                //
                // Add the primary group leader to the list.
                //
                leaders.Add(group.Leader);

                //
                // Make a list of all the assistant leaders.
                //
                if (AssistantLeaderRoles.Length > 0)
                {
                    foreach (GroupMember gm in group.Members)
                    {
                        if (gm.Active && AssistantLeaderRoles.Contains(gm.Role.LookupID))
                            leaders.Add(gm);
                    }
                }

                //
                // Send an e-mail to each person in the list.
                //
                foreach (Person leader in leaders)
                {
                    AttendanceReminder reminder = new AttendanceReminder();
                    Dictionary<string, string> fields = new Dictionary<string, string>();

                    if (Debug)
                        _message.AppendFormat("Sending e-mail to leader '{1}' of Small Group '{0}' which meets on {2}\r\n", new object[] { group.Name, leader.FullName, group.MeetingDay.Value });

                    reminder.LoadPersonFields(fields, leader);

                    //
                    // Send an e-mail to the first active e-mail address the leader has.
                    //
                    foreach (PersonEmail email in leader.Emails)
                    {
                        if (email.Active)
                        {
                            //
                            // Send the e-mail and then stop looking for a valid e-mail address.
                            //
//                            reminder.Send(email.Email, fields);

                            break;
                        }
                    }
                }
            }

            return true;
        }
    }
}
