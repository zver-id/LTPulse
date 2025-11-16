import styles from "./MainScreen.module.css"
import Navigation from "../navigation/Navigation.tsx";
import MonthLineChart from "../lineChart/monthLineChart.jsx";
import {useState} from "react";

function Main() {
    const [team, setTeam] = useState(1)
    const [dayCount, setDayCount] = useState(30)
    return (<>
            <header>
                <h1 className={styles.siteTitle}>Статистика</h1>
            </header>
            <Navigation onChange={setTeam} />
            <MonthLineChart teamId={team} dayCount={dayCount} />
        </>
    )
}

export default Main