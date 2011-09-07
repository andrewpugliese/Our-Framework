--
-- Task Procesing Queue
--
CREATE TABLE B1.TaskProcessingQueue
(
	TaskQueueCode			INT NOT NULL,			-- Unique identifier of this task queue record
	TaskId					NVARCHAR(64) NOT NULL,	-- Unique identifier of the task definition
	StatusCode				TINYINT NOT NULL DEFAULT(0), -- The status of the task queue record
														 -- 0: NotQueued; 32: Queued; 64: InProcess; 128: Failed; 255: Succeeded
	PriorityCode			TINYINT NOT NULL DEFAULT(32), -- The priority code of the task item (any number 0 - 255)
														-- items are dequeued in PriorityCode order.
	StatusDateTime			DATETIME NOT NULL DEFAULT(GETUTCDATE()), -- The datetime of the last status change
	WaitForDateTime			DATETIME NOT NULL DEFAULT(GETUTCDATE()),-- The earliest datetime that the task can be processed 
	WaitForTasks			BIT NOT NULL DEFAULT(0), -- Indicates that this task has dependancies
	WaitForNoUsers			BIT NOT NULL DEFAULT(0), -- indicates that task item cannot be processed until all users are off system
	ClearParametersAtEnd	BIT NOT NULL DEFAULT(0), -- Indicates whether or not to clear the TaskParameters column on completion
	IntervalCount			INT NOT NULL DEFAULT(0), -- The current interval iteration of this task when not null
	IntervalSecondsRequeue	INT NOT NULL DEFAULT(0), -- The new WaitForDateTime (competedDateTime + IntervalSeconds) for automatic requeue
	StatusMsg				NVARCHAR(512),	-- The status msg for this task queue record
	StartedDateTime			DATETIME,		-- The datetime when the task last started
	CompletedDateTime		DATETIME,		-- The datetime when the task last completed (Failed or Succeeded)
	ProcessEngineId			NVARCHAR(32),	-- The EngineId that last processed (or currently processing) the queue item
	LastCompletedCode		TINYINT,		-- The status code of the last completion (Failed or Succeeded)
	LastCompletedMsg		NVARCHAR(512),	-- The status msg of the completion
	WaitForEngineId			NVARCHAR(32),	-- The only Engine process that can process this task
	WaitForConfigId			NVARCHAR(32),	-- The only configuration that this task must be processed under
	TaskParameters			NVARCHAR(512),	-- String that will be passed to task function
	TaskRemark				NVARCHAR(512),	-- Optional remark about the task item
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT TaskProcessingQueue_PK_TaskQueueCode 
		PRIMARY KEY (TaskQueueCode) ,
	CONSTRAINT TaskProcsssingQueue_FK_TaskRegistrations 
		FOREIGN KEY (TaskId) REFERENCES B1.TaskRegistrations(TaskId),
	CONSTRAINT TaskProcsssingQueue_FK_TaskStatusCodes
		FOREIGN KEY (StatusCode) REFERENCES B1.TaskStatusCodes(StatusCode),
	CONSTRAINT TaskProcsssingQueue_FK_AppMaster_ProcessEngineId
		FOREIGN KEY (ProcessEngineId) REFERENCES B1.AppMaster (AppId),
	CONSTRAINT TaskProcsssingQueue_FK_AppMaster_WaitForEngineId 
		FOREIGN KEY (WaitForEngineId) REFERENCES B1.AppMaster (AppId),
	CONSTRAINT TaskProcsssingQueue_FK_TaskConfigurations 
		FOREIGN KEY (WaitForConfigId) REFERENCES B1.TaskConfigurations(ConfigId),
	CONSTRAINT TaskProcsssingQueue_FK_UserMaster_Code
		FOREIGN KEY (LastModifiedUserCode) REFERENCES B1.UserMaster(UserCode),
	CONSTRAINT TaskProcsssingQueue_CC_LastCompletedCode
	CHECK (LastCompletedCode >= 128),
	CONSTRAINT TaskProcsssingQueue_CC_IntervalCount
	CHECK (IntervalCount >= 0),
	CONSTRAINT TaskProcsssingQueue_CC_IntervalSecondsRequeue
	CHECK (IntervalSecondsRequeue >= 0),
) ON B1Core

GO
  
CREATE UNIQUE INDEX TaskProcessingQueue_UX_Priority
ON B1.TaskProcessingQueue
(
	StatusCode
	, PriorityCode
	, WaitForDateTime
	, WaitForTasks
	, WaitForNoUsers
	, WaitForEngineId
	, WaitForConfigId
	, TaskQueueCode
) ON B1CoreIdx

GO

CREATE UNIQUE INDEX TaskProcessingQueue_UX_TaskId
ON B1.TaskProcessingQueue
(
	TaskId
	, TaskQueueCode
) ON B1CoreIdx

GO

CREATE UNIQUE INDEX TaskProcessingQueue_UX_Status
ON B1.TaskProcessingQueue
(
	StatusCode
	, StatusDateTime
	, TaskQueueCode
) ON B1CoreIdx

GO