INSERT INTO public.employee_teams(
	employee_id, team_id)
	SELECT id, team_id FROM public."Employee"
