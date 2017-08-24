DO
$$
BEGIN
  raise notice 'Migration from 1 to 2';
	if (select version from dbversion limit 1) = 1 then

  raise notice 'Started';

  ALTER TABLE Taskuser RENAME COLUMN firebaseid TO ProviderId;

  raise notice 'Altered Taskuser';

  update dbversion
  set version = 2
  where version = 1;

  end if;

  raise notice 'Complete';
END
$$
