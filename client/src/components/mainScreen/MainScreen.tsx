import styles from "./MainScreen.module.css"
import Navigation from "../navigation/Navigation.tsx";
import MonthLineChart from "../lineChart/monthLineChart.jsx";
import {useState} from "react";
import ZonesChart from "../areaChart/ZonesChart.tsx";
import type {zonesColor} from "../../types/zones-chart-props.ts";
import TicketScreen from "../ticketsScreen/ticketScreen.tsx";
import { useLocalStorageState } from "../../storage/useLocalStorageState.ts"

function Main() {
    const [team, setTeam] = useLocalStorageState("team", 1)
    const [dayCount, setDayCount] = useLocalStorageState("dayCount", 140)
    const [viewMode, setViewMode] = useState('charts')
    const [ticketsType, setTicketsType] = useState('')

    const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];
    const zoneColorSimple: zonesColor[] = [ {name: '0-8', color: colors[0]},
        {name: '8-16', color: colors[1]},
        {name: '16-24', color: colors[2]},
        {name: '>24', color: colors[3]}]

    return (<>
            <header>
                <h1 className={styles.siteTitle}>Статистика</h1>
                <Navigation onChangeTeam={setTeam} days={dayCount} onChangeDays={setDayCount} teamId={team}/>

                <div className={styles.buttonGroup}>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'charts' ? styles.activeButton : ''}`}
                        onClick={() => setViewMode('charts')}
                    >
                        Графики
                    </button>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'tickets' && ticketsType === 'incidents' ? styles.activeButton : ''}`}
                        onClick={() => {
                            setViewMode('tickets');
                            setTicketsType("incidents");
                        }}
                    >
                        Инциденты
                    </button>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'tickets' && ticketsType === 'consultations' ? styles.activeButton : ''}`}
                        onClick={() => {
                            setViewMode('tickets');
                            setTicketsType("consultations");
                        }}
                    >
                        Консультации
                    </button>
                    <button
                        className={`${styles.toggleButton} ${viewMode === 'tickets' && ticketsType === 'requests' ? styles.activeButton : ''}`}
                        onClick={() => {
                            setViewMode('tickets');
                            setTicketsType("requests");
                        }}
                    >
                        Запросы
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
                <TicketScreen teamId={team} ticketType={ticketsType} />
            )}
        </>
    )
}

export default Main