import React, { useEffect } from 'react';
import { Modal, Form, Input, message } from 'antd';
import type {ITicket} from '../ticketsScreen/ticketScreen.tsx';
import TextArea from "antd/es/input/TextArea";

interface EditUserModalProps {
    isOpen: boolean;
    ticket: ITicket | null;
    loading: boolean;           // Состояние загрузки при сохранении
    onClose: () => void;        // Функция закрытия окна
    onSuccess: () => void;      // Функция успешного сохранения (обновляет таблицу)
}

const EditTicketModal: React.FC<EditUserModalProps> = ({ isOpen, ticket, loading, onClose, onSuccess}) => {

    const [form] = Form.useForm();

    useEffect(() => {
        if (ticket && isOpen) {
            form.setFieldsValue({
                number: ticket.key,
                name: ticket.name,
                org: ticket.organization,
            });
        } else if (!isOpen) {
            form.resetFields();
        }
    }, [ticket, isOpen, form]);

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();

            // Имитация API запроса (замените на реальный)
            const response = await fetch(`/api/users/${ticket?.key}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(values),
            });

            if (!response.ok) throw new Error('Ошибка сохранения');

            message.success('Пользователь успешно обновлен!');
            onSuccess(); // Обновляем таблицу
            onClose();   // Закрываем окно

        } catch (error) {
            console.error('Ошибка:', error);
            message.error('Не удалось сохранить изменения');
        }
    };

    return (
        <Modal
            title={`Обращение ${ticket?.key}`}
            open={isOpen}
            onOk={handleSubmit}
            onCancel={onClose}
            confirmLoading={loading}
            okText="Сохранить"
            cancelText="Отмена"
            width={600}
            destroyOnClose // Уничтожает содержимое при закрытии
        >
            <Form
                form={form}
                layout="vertical"
                name="editTicketForm"
            >
                <Form.Item name="name" label="Описание">
                    <Input disabled />
                </Form.Item>

                <Form.Item name="org" label="Организация">
                    <Input disabled />
                </Form.Item>

                <Form.Item name="email" label="Комментарий">
                    <TextArea autoSize/>
                </Form.Item>

            </Form>
        </Modal>
    );
};

export default EditTicketModal;