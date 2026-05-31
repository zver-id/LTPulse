import {Button, Flex} from "antd";
import styles from "./metricDetails.module.css"

export interface IMetricDetailsItem {
  title: string;
  teamId: number;
}

interface IMetricDetailsItems {
  items: IMetricDetailsItem[];
}

function MetricDetails({items}: IMetricDetailsItems) {
  const onClickHandle = () => {}
  return (
      <Flex className="ant-flex-vertical">
        { items.map((item, index) => {
          return (
              <Button
                  className={styles.button}
                  onClick={onClickHandle}
                  key={index}
              >{item.title}</Button>
          )
        })}
      </Flex>
  )
}

export default MetricDetails;