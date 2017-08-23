CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE TaskPrototype
(
UUID UUID PRIMARY KEY,
CronExpression varchar(25)
);

ALTER TABLE Task
ADD TaskPrototype UUID NOT NULL DEFAULT uuid_generate_v4();

insert into TaskPrototype
select TaskPrototype from Task;

ALTER TABLE Task
ALTER COLUMN TaskPrototype
DROP DEFAULT;

ALTER TABLE Task
ADD CONSTRAINT task_taskprototype_fkey
FOREIGN KEY (TaskPrototype)
REFERENCES TaskPrototype (UUID) ON DELETE CASCADE;

update dbversion
set version = 3
where version = 2;
