DO
$$
BEGIN
  raise notice 'Initial DB initializetion';
	if (select version from dbversion limit 1) = 0 then

  raise notice 'Start';

  CREATE TABLE TaskUser (
  UUID UUID PRIMARY KEY,
  FirebaseId varchar(50) NOT NULL UNIQUE,
  CreationDate timestamptz NOT NULL,
  FirstName varchar(255),
  LastName varchar(255),
  Email varchar(255) NOT NULL
  );

  raise notice 'Created TaskUser';

  CREATE INDEX FirebaseId_Email_Index ON TaskUser (FirebaseId, Email);

  raise notice 'Created FirebaseId_Email_Index';

  CREATE TABLE Task (
  UUID UUID PRIMARY KEY,
  Completed BOOL NOT NULL,
  Description varchar(255) NOT NULL,
  Notes varchar(4000),
  CreationDate timestamptz NOT NULL,
  TargetDate timestamptz,
  TargetDateIncludeTime BOOL NOT NULL,
  Owner UUID NOT NULL REFERENCES TaskUser (UUID) ON DELETE CASCADE
  );

  raise notice 'Created Task';

  CREATE TABLE PushNotification
  (
  UUID UUID PRIMARY KEY,
  ServiceId UUID NOT NULL,
  TaskId UUID NOT NULL REFERENCES Task (UUID) ON DELETE CASCADE,
  UserId UUID NOT NULL REFERENCES TaskUser (UUID) ON DELETE CASCADE,
  DueDate timestamptz NOT NULL
  );

  raise notice 'Created PushNotification';

  CREATE INDEX PushNotification_ServiceId ON PushNotification (ServiceId);

  raise notice 'Created PushNotification_ServiceId';

  update dbversion
  set version = 1
  where version = 0;

  end if;

  raise notice 'Complete';

END
$$
