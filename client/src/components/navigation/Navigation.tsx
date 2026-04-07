import styles from "./navigation.module.css"
import { useGetAllTeamsQuery } from "../../storage/services/teams-api.ts"

export interface INavigationProps {
    onChangeTeam: (teams: number) => void;
    days: number;
    onChangeDays: (days: number) => void;
    teamId: number;
}

function Navigation (props: INavigationProps){

    const { onChangeDays, days, onChangeTeam, teamId } = props
    const {data} = useGetAllTeamsQuery()

    const handleChangeTeam = (event: React.ChangeEvent<HTMLSelectElement>) => {
        onChangeTeam(Number(event.target.value))
    }

    const handleChangeDays = (event: React.ChangeEvent<HTMLInputElement>) => {
        onChangeDays(Number(event.target.value))
    }

    return(
        <nav>
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
                    </option> )}
            </select>
            <input
                type="text"
                value={days}
                onChange={handleChangeDays}
                placeholder="Количество дней"
                className="input-style" // добавьте свои стили
            />
        </nav>
    )
}

export default Navigation