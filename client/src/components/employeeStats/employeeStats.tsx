import {Card, ConfigProvider, DatePicker, Flex, Select, Table, Typography} from 'antd';
import ruRU from 'antd/locale/ru_RU';
import type {ColumnsType} from 'antd/es/table';
import {useState} from 'react';
import dayjs, {Dayjs} from 'dayjs';
import {useGetEmployeeStatsQuery} from '../../storage/services/employee-stats-api.ts';
import {useGetAllTeamsQuery} from '../../storage/services/teams-api.ts';

import type {Locale} from 'antd/es/locale';
const {RangePicker} = DatePicker;

const ruLocale: Locale = {
  ...ruRU,
  weekStartOnMonday: true,
  DatePicker: {
    ...ruRU.DatePicker,
    lang: {
      ...ruRU.DatePicker?.lang,
      shortWeekDays: ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'],
    },
  },
} as Locale;

export interface IEmployeeStats {
  employee: string;
  assigned: number;
  closed: number;
  backlog: number;
  avgResolutionTime: number;
  slaResolution: number | null;
  slaReaction: number | null;
  responseCount: number | null;
  escalationLine: number | null;
  escalationDevs: number | null;
  criticalZones: number;
  orangeZones: number;
  yellowZones: number;
  greenZones: number;
  older2Weeks: number;
  older3Weeks: number;
  older1Month: number;
  gradeScore: number | null;
}

const formatPercent = (v: number | null) => v === null ? '—' : `${v.toLocaleString('ru-RU')}%`;
const formatNumber = (v: number) => v.toLocaleString('ru-RU');

export const employeeStatsColumns: ColumnsType<IEmployeeStats> = [
  {title: 'Сотрудник', dataIndex: 'employee', key: 'employee', fixed: 'left', width: 160,
    sorter: (a, b) => a.employee.localeCompare(b.employee, 'ru')},
  {title: 'Назначено', dataIndex: 'assigned', key: 'assigned', width: 100,
    sorter: (a, b) => a.assigned - b.assigned, render: formatNumber},
  {title: 'Закрыто', dataIndex: 'closed', key: 'closed', width: 90,
    sorter: (a, b) => a.closed - b.closed, render: formatNumber},
  {title: 'Бэклог', dataIndex: 'backlog', key: 'backlog', width: 90,
    sorter: (a, b) => a.backlog - b.backlog, render: formatNumber},
  {title: 'Ср. время решения/ч', dataIndex: 'avgResolutionTime', key: 'avgResolutionTime', width: 150,
    sorter: (a, b) => a.avgResolutionTime - b.avgResolutionTime,
    render: (v: number) => v.toLocaleString('ru-RU', {maximumFractionDigits: 1})},
  {title: 'SLA_Решение_%', dataIndex: 'slaResolution', key: 'slaResolution', width: 120,
    sorter: (a, b) => (a.slaResolution ?? -1) - (b.slaResolution ?? -1), render: formatPercent},
  {title: 'SLA_Реакция_%', dataIndex: 'slaReaction', key: 'slaReaction', width: 120, render: formatPercent},
  {title: 'Кол-во ответов_%', dataIndex: 'responseCount', key: 'responseCount', width: 140, render: formatPercent},
  {title: 'Эскалация на линию_%', dataIndex: 'escalationLine', key: 'escalationLine', width: 160,
    sorter: (a, b) => (a.escalationLine ?? -1) - (b.escalationLine ?? -1), render: formatPercent},
  {title: 'Эскалация на разработчиков_%', dataIndex: 'escalationDevs', key: 'escalationDevs', width: 190,
    sorter: (a, b) => (a.escalationDevs ?? -1) - (b.escalationDevs ?? -1), render: formatPercent},
  {title: 'КЗ(>=24)', dataIndex: 'criticalZones', key: 'criticalZones', width: 90,
    sorter: (a, b) => a.criticalZones - b.criticalZones, render: formatNumber},
  {title: 'ОЗ(>=16)', dataIndex: 'orangeZones', key: 'orangeZones', width: 90,
    sorter: (a, b) => a.orangeZones - b.orangeZones, render: formatNumber},
  {title: 'ЖЗ(>=8)', dataIndex: 'yellowZones', key: 'yellowZones', width: 90,
    sorter: (a, b) => a.yellowZones - b.yellowZones, render: formatNumber},
  {title: 'ЗЗ(<8)', dataIndex: 'greenZones', key: 'greenZones', width: 90,
    sorter: (a, b) => a.greenZones - b.greenZones, render: formatNumber},
  {title: 'Старше 2 недель', dataIndex: 'older2Weeks', key: 'older2Weeks', width: 130,
    sorter: (a, b) => a.older2Weeks - b.older2Weeks, render: formatNumber},
  {title: 'Старше 3 недель', dataIndex: 'older3Weeks', key: 'older3Weeks', width: 130,
    sorter: (a, b) => a.older3Weeks - b.older3Weeks, render: formatNumber},
  {title: 'Старше месяца', dataIndex: 'older1Month', key: 'older1Month', width: 120,
    sorter: (a, b) => a.older1Month - b.older1Month, render: formatNumber},
  {title: 'Оценка_%', dataIndex: 'gradeScore', key: 'gradeScore', width: 100,
    sorter: (a, b) => (a.gradeScore ?? -1) - (b.gradeScore ?? -1), render: formatPercent},
];

function computeTotals(rows: IEmployeeStats[]) {
  const n = rows.length;
  const sum = (f: (r: IEmployeeStats) => number) => rows.reduce((s, r) => s + f(r), 0);
  const avg = (f: (r: IEmployeeStats) => number | null) => {
    const vals = rows.map(f).filter((v): v is number => v !== null);
    return vals.length > 0 ? Math.round(vals.reduce((s, v) => s + v, 0) / vals.length * 10) / 10 : null;
  };
  return {
    employee: 'Итого / среднее',
    assigned: sum(r => r.assigned),
    closed: sum(r => r.closed),
    backlog: sum(r => r.backlog),
    avgResolutionTime: n > 0 ? Math.round(rows.reduce((s, r) => s + r.avgResolutionTime, 0) / n * 10) / 10 : 0,
    slaResolution: avg(r => r.slaResolution),
    slaReaction: avg(r => r.slaReaction),
    responseCount: avg(r => r.responseCount),
    escalationLine: avg(r => r.escalationLine),
    escalationDevs: avg(r => r.escalationDevs),
    criticalZones: sum(r => r.criticalZones),
    orangeZones: sum(r => r.orangeZones),
    yellowZones: sum(r => r.yellowZones),
    greenZones: sum(r => r.greenZones),
    older2Weeks: sum(r => r.older2Weeks),
    older3Weeks: sum(r => r.older3Weeks),
    older1Month: sum(r => r.older1Month),
    gradeScore: avg(r => r.gradeScore),
  } as IEmployeeStats;
}

function EmployeeStats() {
  const {data: teams} = useGetAllTeamsQuery();
  const [teamId, setTeamId] = useState<number | undefined>(undefined);
  const [range, setRange] = useState<[Dayjs, Dayjs] | null>([dayjs().startOf('month'), dayjs()]);

  const beginDate = range?.[0]?.format('YYYY-MM-DD') ?? dayjs().startOf('month').format('YYYY-MM-DD');
  const endDate = range?.[1]?.format('YYYY-MM-DD') ?? dayjs().format('YYYY-MM-DD');

  const {data: stats, isLoading} = useGetEmployeeStatsQuery(
    {teamId: teamId ?? 0, beginDate, endDate},
    {skip: teamId === undefined}
  );

  const dataWithTotal = stats && stats.length > 0 ? [...stats, computeTotals(stats)] : [];

  return (
    <div>
      <Flex gap="middle" style={{marginBottom: 16}}>
        <Select
          placeholder="Выберите команду"
          style={{width: 250}}
          value={teamId}
          onChange={setTeamId}
          options={teams?.map(t => ({value: t.id, label: t.name}))}
        />
        <ConfigProvider locale={ruLocale}>
          <RangePicker
            value={range}
            onChange={(value) => setRange(value as [Dayjs, Dayjs] | null)}
            format="DD-MM-YYYY"
          />
        </ConfigProvider>
      </Flex>
      {isLoading && <Typography>Загрузка...</Typography>}
      {!isLoading && stats && stats.length === 0 && (
        <Card><Typography>Нет данных.</Typography></Card>
      )}
      {!isLoading && stats && stats.length > 0 && (
        <Table
          dataSource={dataWithTotal.map((r, i) => ({...r, key: i}))}
          columns={employeeStatsColumns}
          scroll={{x: 2200}}
          pagination={false}
          size="small"
          summary={() => null}
        />
      )}
    </div>
  );
}

export default EmployeeStats;
