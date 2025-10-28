import styles from "./navigation.module.css"
import {teams} from "../../testData/teams.ts";
import type {NavigationProps} from "../../types/navigation.ts";

function Navigation (props: NavigationProps){

    const { onChange } = props

    const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        onChange(event.target.value)
    }

    return(
        <nav>
            <label htmlFor={"teamSelection"}>Выбери команду</label>
            <select className={styles.selectBar}
                    id={"teamSelection"}
                    name={"teams"}
                    onChange={handleChange}>
                {teams.map((team) =>
                    <option
                        value={team.value}
                        key={team.value}>
                        {team.name}
                    </option> )}
            </select>
        </nav>
    )
}

export default Navigation