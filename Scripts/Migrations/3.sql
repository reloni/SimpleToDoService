DO
$$
BEGIN
  raise notice 'Migration from 3 to 4';
	if (select version from dbversion limit 1) = 3 then

  raise notice 'Started';

  ALTER TABLE TaskPrototype ALTER COLUMN CronExpression TYPE varchar (255)

  raise notice 'Altered TaskPrototype';
  raise notice 'Change CronExpression TYPE to varchar (255)';

  update dbversion
  set version = 4
  where version = 3;

  end if;

  raise notice 'Complete';
END
$$
