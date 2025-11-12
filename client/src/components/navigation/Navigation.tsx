import styles from "./navigation.module.css"
import type {NavigationProps} from "../../types/navigation.ts";
import { useGetAllTeamsQuery } from "../../storage/services/teams-api.ts"

function Navigation (props: NavigationProps){

    const { onChange } = props
    const {data} = useGetAllTeamsQuery()

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
                {data?.map((team) =>
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