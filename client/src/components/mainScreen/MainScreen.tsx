import styles from "./MainScreen.module.css"
import Navigation from "../navigation/Navigation.tsx";
import MonthLineChart from "../lineChart/monthLineChart.jsx";
import {useState} from "react";
import ZonesChart from "../areaChart/ZonesChart.tsx";
import type {zonesColor} from "../../types/zones-chart-props.ts";
import TicketScreen from "../ticketsScreen/ticketScreen.tsx";

function Main() {
    const [team, setTeam] = useState(1)
    const [dayCount, setDayCount] = useState(14)
    const [viewMode, setViewMode] = useState('charts')

    const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];
    const zoneColorSimple: zonesColor[] = [ {name: '0-8', color: colors[0]},
        {name: '8-16', color: colors[1]},
        {name: '16-24', color: colors[2]},
        {name: '>24', color: colors[3]}]

    return (<>
            <header>
                <h1 className={styles.siteTitle}>Статистика</h1>
                <Navigation onChangeTeam={setTeam} days={dayCount} onChangeDays={setDayCount} />

                <div className={styles.buttonGroup}>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'charts' ? styles.activeButton : ''}`}
                        onClick={() => setViewMode('charts')}
                    >
                        Графики
                    </button>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'tickets' ? styles.activeButton : ''}`}
                        onClick={() => setViewMode('tickets')}
                    >
                        Тикеты
                    </button>
                </div>
            </header>

            {viewMode === 'charts' && (
                <>
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"month"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"Older3Week"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"tail"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"snowball"} />
                    <ZonesChart teamId={team} dayCount={dayCount} filter={"ColorZones"} zoneColor={zoneColorSimple} />
                </>
            )}

            {viewMode === 'tickets' && (
                <TicketScreen />
            )}
        </>
    )
}

export default Main