import {Select} from "antd";
import {Link} from "react-router-dom";
import styles from "./TeamsSettings.module.css";
import {useLocalStorageState} from "../../storage/useLocalStorageState.ts";
import {useGetAllTeamsQuery} from "../../storage/services/teams-api.ts";
//import TeamEmployeeSheet from "../teamEmployeeSheet/teamEmployeeSheet.tsx";

function TeamsSettings(){
  const [team, setTeam] = useLocalStorageState<number>("team", 1)
  const {data} = useGetAllTeamsQuery()

  const safeTeam = team as number

  return <>
    <header className="teamsSettingsHeader">
      <h1 className={styles.siteTitle}>Настройки команд </h1>
      <Select
        className={styles.select}
        value={safeTeam}
        onChange={(value) => setTeam(value as number)}
        options={data?.map(team => ({
          value: team.id,
          label: team.name
        }))}
      />
      <Link className={styles.employeeLink} to={"/employee-teams"}>Настройки сотрудников</Link>
    </header>
    <main className="teamsSettingsMain">
      {/*<TeamEmployeeSheet teamId={safeTeam}>*/}
    </main>
  </>
}

export default TeamsSettings