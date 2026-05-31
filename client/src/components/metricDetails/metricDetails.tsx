import {Button, Flex} from "antd";
import styles from "./metricDetails.module.css"
import {useState} from "react";
import MetricDetailsModal from "../metricDetailsModal/metricDetailsModal.tsx";

export interface IMetricDetailsItem {
  title: string;
  teamId: number;
}

interface IMetricDetailsItems {
  items: IMetricDetailsItem[];
}

function MetricDetails({items}: IMetricDetailsItems) {
  const [isOpenModal, setOpenModal] = useState(false);
  const onClickHandle = () => {setOpenModal(true)}
  const onClose = () => {setOpenModal(false)}
  const today = new Date().toISOString().split('T')[0];

  return (
      <Flex className="ant-flex-vertical">
        { items.map((item, index) => {
          return (
              <>
              <Button
                  className={styles.button}
                  onClick={onClickHandle}
                  key={index}
              >{item.title}</Button>
              <MetricDetailsModal
                  teamId={item.teamId}
                  date={today}
                  metric={item.title}
                  isOpen={isOpenModal}
                  onClose={onClose}/>
              </>
          )
        })}
      </Flex>
  )
}

export default MetricDetails;