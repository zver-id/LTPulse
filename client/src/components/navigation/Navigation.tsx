import styles from "./navigation.module.css"
import {useGetAllTeamsQuery} from "../../storage/services/teams-api.ts"
import dayjs from 'dayjs';

export interface INavigationProps {
  onChangeTeam: (teams: number) => void;
  days: number;
  onChangeDays: (days: number) => void;
  teamId: number;
}

function Navigation(props: INavigationProps) {

  const {onChangeDays, days, onChangeTeam, teamId} = props
  const {data} = useGetAllTeamsQuery()

  const selectedTeam = data?.find((t) => t.id === teamId);
  const lastCalcLabel = selectedTeam?.lastMetricsCalculated
    ? `Метрики рассчитаны: ${dayjs(selectedTeam.lastMetricsCalculated).format('DD.MM.YYYY HH:mm')}`
    : 'Метрики ещё не рассчитаны';

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
          value={days}
          onChange={handleChangeDays}
          placeholder="Количество дней"
        />
      </div>
      <span className={styles.lastCalc}>{lastCalcLabel}</span>
    </nav>
  )
}

export default Navigation
