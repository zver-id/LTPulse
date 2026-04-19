import styles from "../mainScreen/MainScreen.module.css";
import {useState} from "react";
import LineTicketChart from "../lineChart/lineTicketChart.tsx";
import ZonesChart from "../areaChart/ZonesChart.tsx";
import MetricBarChart from "../barChart/barChart.tsx"
import TicketScreen from "../ticketsScreen/ticketScreen.tsx";
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
                    <LineTicketChart teamId={team} dayCount={dayCount} filter={name}
                                     nameOfChart={nameOfChart} key ={id}/>
                  )
                }
                else if (chartType === 'Area') {
                  return (
                    <ZonesChart teamId={team} dayCount={dayCount} filter={name} nameOfChart={nameOfChart} key={id}/>
                  )
                }
                else if (chartType === 'Column') {
                  return (
                    <MetricBarChart teamId={team} dayCount={dayCount} filter={name} nameOfChart={nameOfChart} key={id}/>
                  )
                }
              })
            }
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