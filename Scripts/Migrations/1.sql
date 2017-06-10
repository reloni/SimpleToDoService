ALTER TABLE Taskuser RENAME COLUMN firebaseid TO ProviderId;

update dbversion
set version = 2
where version = 1;
