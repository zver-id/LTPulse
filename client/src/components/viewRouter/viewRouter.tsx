import styles from "../mainScreen/MainScreen.module.css";
import {useState} from "react";
import MonthLineChart from "../lineChart/monthLineChart.tsx";
import ZonesChart from "../areaChart/ZonesChart.tsx";
import MetricBarChart from "../barChart/barChart.tsx"
import TicketScreen from "../ticketsScreen/ticketScreen.tsx";
import type {zonesColor} from "../../types/zones-chart-props.ts";
import GradeScreen from "../gradeScreen/gradeScreen.tsx";
import {type IMetricGroup, useGetMetricGroupsQuery} from "../../storage/services/metricGroups-api.ts";

export interface IViewRouterProps {
  team: number;
  dayCount: number;
}

const ViewRouter = ({team, dayCount}: IViewRouterProps) => {
  const [viewMode, setViewMode] = useState('charts')
  const [ticketsType, setTicketsType] = useState('')
  const { data: metricGroups } = useGetMetricGroupsQuery()

  const buttons = [
    {label: 'Графики', mode: 'charts', type: ''},
    {label: 'Инциденты', mode: 'tickets', type: 'Инцидент'},
    {label: 'Консультации', mode: 'tickets', type: 'Консультация'},
    {label: 'Запросы', mode: 'tickets', type: 'Запрос на обслуживание'},
    {label: 'Старше 4 недель', mode: 'tickets', type: 'Старше 4 недель'},
    {label: 'Старше 3 недель', mode: 'tickets', type: 'Старше 3 недель'},
    {label: 'Старше 2 недель', mode: 'tickets', type: 'Старше 2 недель'},
    {label: 'Оценки', mode: 'grades', type: ''}
  ];

  const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];
  const zoneColorSimple: zonesColor[] = [
     {name: '>24', color: colors[3]},
     {name: '16-24', color: colors[2]},
     {name: '8-16', color: colors[1]},
     {name: '0-8', color: colors[0]}
 ]

  const zoneColorPriority: zonesColor[] = [
     {name: '>0.75', color: colors[3]},
     {name: '0.5-0.75', color: colors[2]},
     {name: '0.25-0.5', color: colors[1]},
     {name: '<0.25', color: colors[0]}
 ]

  return (
    <>
      <div className={styles.buttonGroup}>
        {
          buttons.map(({label, mode, type}) => (
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
            {
              metricGroups?.map(({id, name, nameOfChart, chartType}: IMetricGroup) => {
                if (chartType === 'Line') {
                  return (
                      <MonthLineChart teamId={team} dayCount={dayCount} filter={name}/>
                  )
                }
                else return <div>{name}</div>;

              })
            }

            <ZonesChart teamId={team} dayCount={dayCount} filter={"ColorZones"} zoneColor={zoneColorSimple}/>
            <ZonesChart teamId={team} dayCount={dayCount} filter={"ColorZonesPriority"} zoneColor={zoneColorPriority}/>
            <MetricBarChart teamId={team} dayCount={dayCount} filter={"NegativeGrades"}/>
            <MetricBarChart teamId={team} dayCount={dayCount} filter={"ExternalMessages"}/>

          </>
        )}

        {viewMode === 'tickets' && (
          <TicketScreen teamId={team} ticketType={ticketsType}/>
        )}

        {viewMode === 'grades' && (
          <GradeScreen teamId={team} />
        )
        }
      </div>
    </>
  )
}

export default ViewRouter;