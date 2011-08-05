INSERT INTO core_communication_template
	([guid], template_name, [subject], html_message, text_message, [type], date_created, created_by, date_modified, modified_by, organization_id, system_flag)
	VALUES
	('070e0a63-7fcc-4a6f-9a26-a6c671e6d033'
	,'Agent | SG Attendance Reminder'
	,'Small Group Attendance Reminder'
	,''
	,''
	,'Arena.Custom.HDC.MiscModules.Communications.AttendanceReminder'
	,GETDATE()
	,'HDC MiscModules'
	,GETDATE()
	,'HDC MiscModules'
	,1
	,1)
