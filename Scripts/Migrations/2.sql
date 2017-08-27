DO
$$
BEGIN
  raise notice 'Migration from 2 to 3';
	if (select version from dbversion limit 1) = 2 then

  CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
  raise notice 'Created EXTENSION uuid-ossp';

  CREATE TABLE TaskPrototype
  (
  UUID UUID PRIMARY KEY,
  CronExpression varchar(25)
  );
  raise notice 'Created TaskPrototype';

  ALTER TABLE Task
  ADD TaskPrototype UUID NOT NULL DEFAULT uuid_generate_v4();
  raise notice 'Altered Task';

  insert into TaskPrototype
  select TaskPrototype from Task;
  raise notice 'Pupulated TaskPrototype with data';

  ALTER TABLE Task
  ALTER COLUMN TaskPrototype
  DROP DEFAULT;
  raise notice 'Altered Task (drop default)';

  ALTER TABLE Task
  ADD CONSTRAINT task_taskprototype_fkey
  FOREIGN KEY (TaskPrototype)
  REFERENCES TaskPrototype (UUID) ON DELETE CASCADE;
  raise notice 'Altered Task (add constraint)';

  update dbversion
  set version = 3
  where version = 2;

  end if;

  raise notice 'Complete';
END
$$
