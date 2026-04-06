import styles from "../mainScreen/MainScreen.module.css";
import {useState} from "react";
import MonthLineChart from "../lineChart/monthLineChart.tsx";
import ZonesChart from "../areaChart/ZonesChart.tsx";
import MetricBarChart from "../barChart/barChart.tsx"
import TicketScreen from "../ticketsScreen/ticketScreen.tsx";
import type {zonesColor} from "../../types/zones-chart-props.ts";

export interface IViewRouterProps {
    team: number;
    dayCount: number;
}

const ViewRouter =  ({team, dayCount} :IViewRouterProps )=> {
    const [viewMode, setViewMode] = useState('charts')
    const [ticketsType, setTicketsType] = useState('')

    const buttons = [
        { label: 'Графики', mode: 'charts', type: '' },
        { label: 'Инциденты', mode: 'tickets', type: 'Инцидент' },
        { label: 'Консультации', mode: 'tickets', type: 'Консультация' },
        { label: 'Запросы', mode: 'tickets', type: 'Запрос на обслуживание' },
        { label: 'Старше 4 недель', mode: 'tickets', type: 'Старше 4 недель'},
        { label: 'Старше 3 недель', mode: 'tickets', type: 'Старше 3 недель'},
        { label: 'Старше 2 недель', mode: 'tickets', type: 'Старше 2 недель'}
    ];

    const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];
    const zoneColorSimple: zonesColor[] = [ {name: '0-8', color: colors[0]},
        {name: '8-16', color: colors[1]},
        {name: '16-24', color: colors[2]},
        {name: '>24', color: colors[3]}]

    return(
        <>
        <div className={styles.buttonGroup}>
            {
                buttons.map(({label, mode, type})=>(
                    <button
                        key={label}
                        className={`${styles.toggleButton} ${viewMode === mode && ticketsType === type ? styles.activeButton : ''}`}
                        onClick={() => {
                            setViewMode(mode);
                            setTicketsType(type);
                        }}
                    >
                        {label}
                    </button>
                ))
            }
        </div>
        <div>
            {viewMode === 'charts' && (
                <>
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"month"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"Older3Week"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"tail"} />
                    <MonthLineChart teamId={team} dayCount={dayCount} filter={"snowball"} />
                    <ZonesChart teamId={team} dayCount={dayCount} filter={"ColorZones"} zoneColor={zoneColorSimple} />
                    <MetricBarChart teamId={team} dayCount={dayCount} filter={"NegativeGrades"} />
                </>
            )}

            {viewMode === 'tickets' && (
                <TicketScreen teamId={team} ticketType={ticketsType} />
            )}
        </div>
        </>
    )
}

export default ViewRouter;