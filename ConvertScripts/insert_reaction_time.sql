UPDATE public."Priority"
	SET timetoreaction=1
	WHERE name='Критический';

UPDATE public."Priority"
	SET timetoreaction=2
	WHERE name='Высокий';

UPDATE public."Priority"
	SET timetoreaction=4
	WHERE name='Средний';

UPDATE public."Priority"
	SET timetoreaction=24
	WHERE name='Низкий';

UPDATE public."Priority"
	SET timetoreaction=40
	WHERE name='Планируемый';

SELECT * FROM public."Priority"
