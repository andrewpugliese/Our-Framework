CREATE TABLE B1.AppConfigParameters(
  ParameterName Varchar2(48) not null,
  ParameterValue Varchar2(256) not null,
 CONSTRAINT PK_AppConfigParameters PRIMARY KEY(ParameterName)
) tablespace B1Core
/
CREATE UNIQUE  INDEX B1.UX_AppConfigParameters ON B1.AppConfigParameters 
(nlssort( ParameterName , 'NLS_SORT=BINARY_CI' )) tablespace B1CoreIdx
/
grant references on B1.AppConfigParameters to public
/
grant insert, select, update, delete on B1.AppConfigParameters to public
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/