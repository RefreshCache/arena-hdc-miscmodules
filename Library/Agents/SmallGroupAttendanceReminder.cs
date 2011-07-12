using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Arena.Core;
using Arena.Portal;
using Arena.SmallGroup;
using Agent;

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
        private String _assistantLeaderRoles = String.Empty;
        private int _categoryID = 1;
        private int _organizationID = 1;
        private Boolean _debug = false;
        private StringBuilder _message = new StringBuilder();

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

        #endregion


        #region Public Properties

        public String Message { get { return _message.ToString(); } }

        #endregion


        #region Agent Settings

        [TextSetting("Cluster Type IDs", "Comma separated list of cluster types to be included in the reminder e-mails. Leave blank for all in system.", false)]
        public String ClusterTypeIDs { get { return _clusterTypeIds; } set { _clusterTypeIds = value; } }

        [BooleanSetting("Assistant Leader Roles", "Small Group Leaders will always get an e-mail. If you wish assistant leaders to also get the e-mail enter the comma separated list of small group role ID numbers that you wish to also receive a reminder e-mail.", true, false)]
        public String NotifyAssistantLeaders { get { return _assistantLeaderRoles; } set { _assistantLeaderRoles = value; } }

        [NumericSetting("Category ID", "Small Group Category to work with when sending e-mail reminders.", true)]
        public int CategoryID { get { return _categoryID; } set { _categoryID = value; } }

        [NumericSetting("Organization ID", "Organization ID to work with for processing e-mail reminders.", true)]
        public int OrganizationID { get { return _organizationID; } set { _organizationID = value; } }

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


        Boolean ProcessGroup(Group group)
        {
            if (Debug)
                _message.AppendFormat("Processing Small Group '{0}' and leader '{1}'\r\n", group.Name, group.Leader.FullName);

            return true;
        }
    }
}
