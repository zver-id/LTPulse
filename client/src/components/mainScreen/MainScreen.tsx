import styles from "./MainScreen.module.css"
import Navigation from "../navigation/Navigation.tsx";
import { useLocalStorageState } from "../../storage/useLocalStorageState.ts"
import ViewRouter from "../viewRouter/viewRouter.tsx";

function Main() {
    const [team, setTeam] = useLocalStorageState("team", 1)
    const [dayCount, setDayCount] = useLocalStorageState("dayCount", 140)

    return (<>
            <header>
                <h1 className={styles.siteTitle}>Статистика</h1>
                <Navigation onChangeTeam={setTeam} days={dayCount} onChangeDays={setDayCount} teamId={team}/>
            </header>
            <ViewRouter team={team} dayCount={dayCount}/>
        </>
    )
}

export default Main