--
-- Task Dependencenies
--
-- Contains the optional relationships between tasks
-- when 1 task depends on other tasks.
-- The records in this table will only be looked at when
-- the TaskProcessQueue record's column: WaitForTaskDateTime is not null
--
CREATE TABLE B1.TaskDependencies
(
	TaskQueueCode			INT NOT NULL,	-- The task item with the dependencies
	WaitTaskQueueCode		INT NOT NULL,	-- The task item to wait for
	WaitTaskCompletionCode	TINYINT NULL,	-- The task item completion code to wait for (Failed, Succeeded, or Both)
											-- NULL; 128: Failed; 255: Succeeded
	LastModifiedUserCode	INT NULL,
	LastModifiedDateTime	DATETIME NULL,
	CONSTRAINT TaskDependencies_PK_TaskQueueCode_WaitTaskQueueCode 
		PRIMARY KEY (TaskQueueCode, WaitTaskQueueCode)
) ON B1Core

GO

ALTER TABLE B1.TaskDependencies
ADD CONSTRAINT TaskDependencies_FK_TaskProcessingQueue_TaskQueueCode
FOREIGN KEY (TaskQueueCode)
REFERENCES B1.TaskProcessingQueue(TaskQueueCode)

GO

ALTER TABLE B1.TaskDependencies
ADD CONSTRAINT TaskDependencies_FK_TaskProcessingQueue_WaitTaskQueueCode
FOREIGN KEY (WaitTaskQueueCode)
REFERENCES B1.TaskProcessingQueue(TaskQueueCode)

GO

ALTER TABLE B1.TaskDependencies
ADD CONSTRAINT TaskDependencies_FK_TaskStatusCodes
FOREIGN KEY (WaitTaskCompletionCode) REFERENCES B1.TaskStatusCodes(StatusCode)

GO

ALTER TABLE B1.TaskDependencies
ADD CONSTRAINT TaskDependencies_FK_UserMaster_Code
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

ALTER TABLE B1.TaskDependencies
ADD CONSTRAINT TaskDependencies_CC_WaitTaskCompletionCode
CHECK (WaitTaskCompletionCode >= 128)

GO