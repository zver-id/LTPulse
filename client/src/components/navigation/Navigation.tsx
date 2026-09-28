import {useEffect, useRef, useState} from "react";
import styles from "./navigation.module.css"
import {useGetAllTeamsQuery, useRecalculateMetricsMutation, teamsApi} from "../../storage/services/teams-api.ts"
import {metricsApi} from "../../storage/services/metrics-api.ts";
import {ticketsApi} from "../../storage/services/tickets-api.ts";
import {gradeApi} from "../../storage/services/grade-api.ts";
import {store} from "../../storage/store.ts";
import type {Team} from "../../types/team.ts";
import dayjs from 'dayjs';

export interface INavigationProps {
  onChangeTeam: (teams: number) => void;
  days: number;
  onChangeDays: (days: number) => void;
  teamId: number;
}

const POLL_INTERVAL_MS = 3000;
const POLL_TIMEOUT_MS = 5 * 60 * 1000;

function Navigation(props: INavigationProps) {

  const {onChangeDays, days, onChangeTeam, teamId} = props
  const {data} = useGetAllTeamsQuery()
  const [recalculate, {isLoading: isRecalculating}] = useRecalculateMetricsMutation()

  const [polling, setPolling] = useState(false)
  const startDateRef = useRef<string | null>(null)
  const pollStartedAt = useRef<number>(Date.now())

  const selectedTeam = data?.find((t) => t.id === teamId);
  const lastCalcLabel = selectedTeam?.lastMetricsCalculated
    ? `Метрики рассчитаны: ${dayjs(selectedTeam.lastMetricsCalculated).format('DD.MM.YYYY HH:mm')}`
    : 'Метрики ещё не рассчитаны';

  // Поллинг: ждём, пока дата последнего расчёта изменится.
  useEffect(() => {
    if (!polling) return;

    const timer = setInterval(async () => {
      const result = await store.dispatch(
        teamsApi.endpoints.getAllTeams.initiate(undefined, {forceRefetch: true})
      );
      const teams = (result as {data?: Team[] | undefined})?.data;
      const team = teams?.find((t) => t.id === teamId);
      const newDate = team?.lastMetricsCalculated ?? null;

      if (startDateRef.current === null) {
        startDateRef.current = newDate;
        return;
      }
      if (newDate !== null && newDate !== startDateRef.current) {
        setPolling(false);
        // Дата изменилась — запрашиваем повторно все данные страницы.
        store.dispatch(metricsApi.util.invalidateTags(["metrics"]));
        store.dispatch(ticketsApi.util.invalidateTags(["tickets", "ticketsByMetric"]));
        store.dispatch(gradeApi.util.invalidateTags(["grades"]));
        return;
      }
      if (Date.now() - pollStartedAt.current > POLL_TIMEOUT_MS) {
        setPolling(false);
      }
    }, POLL_INTERVAL_MS);

    return () => clearInterval(timer);
  }, [polling, teamId]);

  const handleRecalculate = async () => {
    if (isRecalculating || polling) return;
    startDateRef.current = selectedTeam?.lastMetricsCalculated ?? null;
    pollStartedAt.current = Date.now();
    setPolling(true);
    try {
      const result = await recalculate(teamId);
      // Мутация инвалидирует только "teams" — остальные кэши сбрасываем явно.
      store.dispatch(metricsApi.util.invalidateTags(["metrics"]));
      store.dispatch(ticketsApi.util.invalidateTags(["tickets", "ticketsByMetric"]));
      store.dispatch(gradeApi.util.invalidateTags(["grades"]));
      if (result.error) {
        setPolling(false);
      }
    } catch {
      setPolling(false);
    }
  };

  const handleChangeTeam = (event: React.ChangeEvent<HTMLSelectElement>) => {
    onChangeTeam(Number(event.target.value))
  }

  const handleChangeDays = (event: React.ChangeEvent<HTMLInputElement>) => {
    onChangeDays(Number(event.target.value))
  }

  return (
    <nav>
      <div className={styles.row}>
        <label htmlFor={"teamSelection"}>Выбери команду</label>
        <select className={styles.selectBar}
                id={"teamSelection"}
                name={"teams"}
                value={teamId}
                onChange={handleChangeTeam}>
          {data?.map((team) =>
            <option
              value={team.id}
              key={team.id}>
              {team.name}
            </option>)}
        </select>
        <input
          type="text"
          className={styles.dayInput}
          value={days}
          onChange={handleChangeDays}
          placeholder="Количество дней"
        />
      </div>
      <div className={styles.row}>
        <span className={styles.lastCalc}>{lastCalcLabel}</span>
        <button
          type="button"
          className={styles.recalcButton}
          onClick={handleRecalculate}
          disabled={isRecalculating || polling}>
          {polling ? 'Пересчёт…' : 'Пересчитать метрики'}
        </button>
      </div>
    </nav>
  )
}

export default Navigation
