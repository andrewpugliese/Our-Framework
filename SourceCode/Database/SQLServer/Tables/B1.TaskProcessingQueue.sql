--
-- Task Procesing Queue
--
CREATE TABLE B1.TaskProcessingQueue
(
	TaskQueueCode			INT NOT NULL,			-- Unique identifier of this task queue record
	TaskId					VARCHAR(64) NOT NULL,	-- Unique identifier of the task definition
	StatusCode				TINYINT NOT NULL DEFAULT(0), -- The status of the task queue record
														 -- 0: NotQueued; 32: Queued; 64: InProcess; 128: Failed; 255: Succeeded
	PriorityCode			TINYINT NOT NULL DEFAULT(32), -- The priority code of the task item (any number 0 - 255)
														-- items are dequeued in PriorityCode order.
	WaitForDateTime			DATETIME NOT NULL DEFAULT(GETUTCDATE()),-- The earliest datetime that the task can be processed 
	WaitForTasks			BIT NOT NULL DEFAULT(0), -- Indicates that this task has dependancies
	WaitForNoUsers			BIT NOT NULL DEFAULT(1), -- indicates that task item cannot be processed until all users are off system
	StatusMsg				VARCHAR(512),	-- The status msg for this task queue record
	StatusDateTime			DATETIME,		-- The datetime of the last status change
	StartedDateTime			DATETIME,		-- The datetime when the task last started
	CompletedDateTime		DATETIME,		-- The datetime when the task last completed (Failed or Succeeded)
	ProcessEngineId			VARCHAR(32),	-- The EngineId that last processed (or currently processing) the queue item
	LastCompletedCode		TINYINT,		-- The status code of the last completion (Failed or Succeeded)
	LastCompletedMsg		VARCHAR(512),	-- The status msg of the completion
	WaitForEngineId			VARCHAR(32),	-- The only Engine process that can process this task
	WaitForConfigId			VARCHAR(32),	-- The only configuration that this task must be processed under
	TaskParameters			VARCHAR(512),	-- String that will be passed to task function
	ClearParametersAtEnd	BIT,			-- Indicates whether or not to clear the TaskParameters column on completion
	IntervalCount			INT,			-- The current interval iteration of this task when not null
	IntervalSecondsRequeue	INT,			-- The new WaitForDateTime (competedDateTime + IntervalSeconds) for automatic requeue
	TaskRemark				VARCHAR(512),	-- Optional remark about the task item
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_TaskProcessingQueue_TaskQueueCode 
		PRIMARY KEY (TaskQueueCode) ,
	CONSTRAINT FK_TaskProcsssingQueue_TaskRegistrations 
		FOREIGN KEY (TaskId) REFERENCES B1.TaskRegistrations(TaskId),
	CONSTRAINT FK_TaskProcsssingQueue_TaskStatusCodes
		FOREIGN KEY (StatusCode) REFERENCES B1.TaskStatusCodes(StatusCode),
	CONSTRAINT FK_TaskProcsssingQueue_AppMaster_ProcessEngineId
		FOREIGN KEY (ProcessEngineId) REFERENCES B1.AppMaster (AppId),
	CONSTRAINT FK_TaskProcsssingQueue_AppMaster_WaitForEngineId 
		FOREIGN KEY (WaitForEngineId) REFERENCES B1.AppMaster (AppId),
	CONSTRAINT FK_TaskProcsssingQueue_TaskConfigurations 
		FOREIGN KEY (WaitForConfigId) REFERENCES B1.TaskConfigurations(ConfigId),
	CONSTRAINT FK_TaskProcsssingQueue_UserMaster_Code
		FOREIGN KEY (LastModifiedUserCode) REFERENCES B1.UserMaster(UserCode),
	CONSTRAINT CC_TaskProcsssingQueue_LastCompletedCode
	CHECK (LastCompletedCode < 128),
	CONSTRAINT CC_TaskProcsssingQueue_IntervalCount
	CHECK (IntervalCount < 0),
	CONSTRAINT CC_TaskProcsssingQueue_IntervalSecondsRequeue
	CHECK (IntervalSecondsRequeue < 0),
) ON B1Core

GO
  
CREATE UNIQUE INDEX UI_TaskProcessingQueue_Priority
ON B1.TaskProcessingQueue
(
	StatusCode
	, PriorityCode
	, WaitForDateTime
	, WaitForTasks
	, WaitForNoUsers
	, WaitForEngineId
	, WaitForConfigId
) ON B1CoreIdx

GO

CREATE UNIQUE INDEX UI_TaskProcessingQueue_TaskId
ON B1.TaskProcessingQueue
(
	TaskId
	, TaskQueueCode
) ON B1CoreIdx

GO

CREATE UNIQUE INDEX UI_TaskProcessingQueue_LastCompleted
ON B1.TaskProcessingQueue
(
	LastCompletedCode
	, CompletedDateTime
	, TaskQueueCode
) ON B1CoreIdx

GO