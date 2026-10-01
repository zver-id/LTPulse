import {useMemo, useState} from "react";
import {Button, Popconfirm, Select, Spin, Table, message} from "antd";
import {Link} from "react-router-dom";
import styles from "./EmployeeTeamsSettings.module.css";
import {useLocalStorageState} from "../../storage/useLocalStorageState.ts";
import {useGetAllTeamsQuery} from "../../storage/services/teams-api.ts";
import {
  useGetEmployeesByTeamQuery,
  useGetEmployeesQuery,
  useAddEmployeeToTeamMutation,
  useRemoveEmployeeFromTeamMutation
} from "../../storage/services/employee-api.ts";

function EmployeeTeamsSettings() {
  const [team, setTeam] = useLocalStorageState<number>("team", 1);
  const {data: teams} = useGetAllTeamsQuery();

  // Не отправляем запрос, пока team не стал валидным числом.
  const hasTeam = typeof team === "number" && Number.isFinite(team);
  const {data: teamEmployees, isFetching: isFetchingTeam} = useGetEmployeesByTeamQuery(
    hasTeam ? {teamId: team} : {teamId: 0},
    {skip: !hasTeam}
  );
  const {data: allEmployees, isFetching: isFetchingAll} = useGetEmployeesQuery();

  const [addEmployee, {isLoading: isAdding}] = useAddEmployeeToTeamMutation();
  const [removeEmployee] = useRemoveEmployeeFromTeamMutation();
  const [selectedEmployee, setSelectedEmployee] = useState<number | null>(null);
  const [messageApi, contextHolder] = message.useMessage();

  // Сотрудники, которых можно добавить в выбранную команду (не входящие в неё).
  const availableOptions = useMemo(() => {
    const inTeam = new Set((teamEmployees ?? []).map((e) => e.id));
    return (allEmployees ?? [])
      .filter((e) => !inTeam.has(e.id))
      .map((e) => ({value: e.id, label: e.name}));
  }, [allEmployees, teamEmployees]);

  const handleAdd = async () => {
    if (selectedEmployee == null) {
      messageApi.warning("Сначала выберите сотрудника");
      return;
    }
    try {
      await addEmployee({teamId: team, employeeId: selectedEmployee}).unwrap();
      messageApi.success("Сотрудник включен в команду");
      setSelectedEmployee(null);
    } catch {
      messageApi.error("Не удалось включить сотрудника");
    }
  };

  const handleRemove = async (employeeId: number) => {
    try {
      await removeEmployee({teamId: team, employeeId}).unwrap();
      messageApi.success("Сотрудник исключен из команды");
    } catch {
      messageApi.error("Не удалось исключить сотрудника");
    }
  };

  const columns = [
    {
      title: "Сотрудник",
      dataIndex: "name",
      key: "name",
      sorter: (a: { name: string }, b: { name: string }) => a.name.localeCompare(b.name, "ru"),
    },
    {
      title: "",
      key: "actions",
      width: 160,
      render: (_: unknown, record: { id: number; name: string }) => (
        <Popconfirm
          title="Исключить сотрудника?"
          description={`Сотрудник «${record.name}» будет исключен из команды.`}
          okText="Исключить"
          cancelText="Отмена"
          okButtonProps={{danger: true}}
          onConfirm={() => handleRemove(record.id)}
        >
          <Button danger size="small">Исключить</Button>
        </Popconfirm>
      ),
    },
  ];

  // Варианты селектора команды: если команды ещё не загружены, показываем
  // запасную подпись, чтобы селектор не выглядел пустым.
  const teamSelectOptions = teams?.map((t) => ({value: t.id, label: t.name})) ??
    (hasTeam ? [{value: team, label: `Команда ${team}`}] : []);

  return (
    <>
      {contextHolder}
      <div className={styles.page}>
        <div className={styles.header}>
          <h1 className={styles.siteTitle}>Настройки сотрудников</h1>
          <div className={styles.teamSelectWrap}>
            <span className={styles.teamSelectLabel}>Команда</span>
            <Select
              className={styles.select}
              value={team}
              onChange={(value) => setTeam(value as number)}
              options={teamSelectOptions}
              notFoundContent={teams ? "Нет команд" : undefined}
            />
          </div>
          <Link className={styles.backLink} to={"/teams"}>Назад к настройкам команд</Link>
        </div>

        <div className={styles.content}>
          <div className={styles.addCard}>
            <p className={styles.addTitle}>Включить сотрудника в команду</p>
            <div className={styles.addRow}>
              <Select
                className={styles.addSelect}
                showSearch
                allowClear
                value={selectedEmployee}
                optionFilterProp="label"
                placeholder="Выберите сотрудника"
                options={availableOptions}
                onChange={(value) => setSelectedEmployee(value ?? null)}
                notFoundContent={isFetchingAll ? <Spin size="small"/> : "Все сотрудники уже в команде"}
              />
              <Button
                type="primary"
                loading={isAdding}
                disabled={selectedEmployee == null}
                onClick={handleAdd}
              >
                Добавить
              </Button>
            </div>
          </div>

          <Table
            className={styles.table}
            columns={columns}
            dataSource={teamEmployees ?? []}
            rowKey="id"
            loading={isFetchingTeam}
            pagination={{pageSize: 15, showSizeChanger: true}}
            locale={{emptyText: "В команде пока нет сотрудников"}}
          />
        </div>
      </div>
    </>
  );
}

export default EmployeeTeamsSettings;
