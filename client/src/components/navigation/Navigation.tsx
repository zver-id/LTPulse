import styles from "./navigation.module.css"
import type {NavigationProps} from "../../types/navigation.ts";
import { useGetAllTeamsQuery } from "../../storage/services/teams-api.ts"

function Navigation (props: NavigationProps){

    const { onChangeDays, days, onChangeTeam } = props
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