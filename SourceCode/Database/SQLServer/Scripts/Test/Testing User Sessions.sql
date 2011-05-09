select * from b1.appsessions

delete from b1.AppSessions
delete from b1.UserSessions where UserCode = 3

select * from b1.usersessions

select * from b1.usermaster

select * from b1.uicontrols

select * from b1.uniqueids

select * from B1.UserMaster um
inner join B1.AccessControlGroups acg
on um.DefaultAccessGroupCode = acg.AccessControlGroupCode
left outer join B1.AccessControlGroupRules acgr
on acg.AccessControlGroupCode = acgr.AccessControlGroupCode

select * from b1.SignonControl

update b1.usermaster set userpassword = null, PasswordSalt = null where UserCode=2


declare @ControlCode TinyInt 
set @ControlCode = 1 
declare @FailedAttemptLimit TinyInt 
set @FailedAttemptLimit = 5 
declare @ForceSignOff Bit 
set @ForceSignOff = False 
declare @LastModifiedDateTime DateTime 
set @LastModifiedDateTime = '2011-04-12 19:04:35.950' 
declare @LastModifiedDateTime_sv DateTime 
set @LastModifiedDateTime_sv = '2011-04-14 07:04:32.064' 
declare @LastModifiedUserCode Int 
set @LastModifiedUserCode = 2 
declare @LastModifiedUserCode_sv Int 
declare @RestrictSignon Bit 
set @RestrictSignon = False 
declare @RestrictSignonMsg NVarChar(max) 
set @RestrictSignonMsg = '' 
declare @SignoffWarningMsg NVarChar(max) 
set @SignoffWarningMsg = '' 
declare @StatusSeconds SmallInt 
set @StatusSeconds = 30 
declare @TimeoutSeconds SmallInt 
set @TimeoutSeconds = 120 
update T1 set T1.TimeoutSeconds = @TimeoutSeconds
, T1.SignoffWarningMsg = @SignoffWarningMsg
, T1.ForceSignOff = @ForceSignOff
, T1.RestrictSignon = @RestrictSignon
, T1.StatusSeconds = @StatusSeconds
, T1.RestrictSignonMsg = @RestrictSignonMsg
, T1.FailedAttemptLimit = @FailedAttemptLimit
, T1.LastModifiedUserCode = @LastModifiedUserCode_sv
, T1.LastModifiedDateTime = @LastModifiedDateTime_sv


 select *
 from 
 B1.SignonControl T1 
where T1.ControlCode = 1 AND (T1.LastModifiedUserCode = 2 OR (2 is NULL AND T1.LastModifiedUserCode is NULL)) 
AND (T1.LastModifiedDateTime = '2011-04-12 19:04:35.950' OR ('2011-04-12 19:04:35.950' is NULL AND T1.LastModifiedDateTime is NULL))




declare @ControlCode TinyInt 
set @ControlCode = 1 
declare @FailedAttemptLimit TinyInt 
set @FailedAttemptLimit = 5 
declare @ForceSignOff Bit 
set @ForceSignOff = False 
declare @LastModifiedDateTime DateTime 
set @LastModifiedDateTime = '2011-04-15 13:04:51.550' 
declare @LastModifiedDateTime_sv DateTime 
set @LastModifiedDateTime_sv = '2011-04-15 13:04:56.511' 
declare @LastModifiedUserCode Int 
declare @LastModifiedUserCode_sv Int 
set @LastModifiedUserCode_sv = 2 
declare @RestrictSignon Bit 
set @RestrictSignon = False 
declare @RestrictSignonMsg NVarChar(max) 
set @RestrictSignonMsg = '' 
declare @SignoffWarningMsg NVarChar(max) 
set @SignoffWarningMsg = '' 
declare @StatusSeconds SmallInt 
set @StatusSeconds = 60 
declare @TimeoutSeconds SmallInt 
set @TimeoutSeconds = 120 
update T1 set T1.TimeoutSeconds = @TimeoutSeconds
, T1.SignoffWarningMsg = @SignoffWarningMsg
, T1.ForceSignOff = @ForceSignOff
, T1.RestrictSignon = @RestrictSignon
, T1.StatusSeconds = @StatusSeconds
, T1.RestrictSignonMsg = @RestrictSignonMsg
, T1.FailedAttemptLimit = @FailedAttemptLimit
, T1.LastModifiedUserCode = @LastModifiedUserCode_sv
, T1.LastModifiedDateTime = @LastModifiedDateTime_sv
 from 
 B1.SignonControl T1 
where T1.ControlCode = @ControlCode AND (T1.LastModifiedUserCode = @LastModifiedUserCode OR (@LastModifiedUserCode is NULL
 AND T1.LastModifiedUserCode is NULL)) AND (T1.LastModifiedDateTime = @LastModifiedDateTime OR (@LastModifiedDateTime is NULL AND T1.LastModifiedDateTime is NULL))
 

 select * 
 from B1.UserSessions us
 cross join B1.SignonControl sc
 where DateAdd(SECOND, sc.StatusSeconds*5000, us.SessionDateTime) > GETUTCDATE()