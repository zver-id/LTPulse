import {Select} from "antd";
import styles from "./TeamsSettings.module.css";
import {useLocalStorageState} from "../../storage/useLocalStorageState.ts";
import {useGetAllTeamsQuery} from "../../storage/services/teams-api.ts";

function TeamsSettings(){
  const [team, setTeam] = useLocalStorageState("team", 1)
  const {data} = useGetAllTeamsQuery()

  return <>
    <header>
      <h1 className={styles.siteTitle}>Настройки команд </h1>
      <Select
        className={styles.select}
        defaultValue={{value: team.id, label: team.name}}
        onChange={setTeam}
        options={data?.map(team => ({
          value: team.id,
          label: team.name
        }))}
      />
    </header>
    <main>

    </main>
  </>
}

export default TeamsSettings