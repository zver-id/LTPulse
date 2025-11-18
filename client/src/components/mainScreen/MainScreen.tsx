import styles from "./MainScreen.module.css"
import Navigation from "../navigation/Navigation.tsx";
import MonthLineChart from "../lineChart/monthLineChart.jsx";
import {useState} from "react";
import ZonesChart from "../areaChart/ZonesChart.tsx";

function Main() {
    const [team, setTeam] = useState(1)
    const [dayCount, setDayCount] = useState(100)
    return (<>
            <header>
                <h1 className={styles.siteTitle}>Статистика</h1>
            </header>
            <Navigation onChange={setTeam} />
            <MonthLineChart teamId={team} dayCount={dayCount} filter={"month"} />
            <MonthLineChart teamId={team} dayCount={dayCount} filter={"Older3Week"} />
            <MonthLineChart teamId={team} dayCount={dayCount} filter={"tail"} />
            <MonthLineChart teamId={team} dayCount={dayCount} filter={"snowball"} />
            <ZonesChart teamId={team} dayCount={dayCount} filter={"ColorZones"} />
        </>
    )
}

export default Main