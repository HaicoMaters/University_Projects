from flask_wtf import FlaskForm, RecaptchaField, Recaptcha
import re
from wtforms import StringField, SubmitField, PasswordField
from wtforms.validators import DataRequired, ValidationError, Email, EqualTo


# Register
# For checking first and last name
def character_check(form, field):
    excluded_chars = "* ? ! ' ^ + % & / ( ) = } ] [ { $ # @ < >"
    for char in field.data:
        if char in excluded_chars:
            raise ValidationError(f"Character {char} is not allowed")


def phone_format(self, field):
    p = re.compile(r"\d{4}\-\d{3}\-\d{4}")
    if not p.match(field.data):
        raise ValidationError("Must be in format XXXX-XXX-XXXX")


def password_format(self, field):
    p = re.compile(r"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\W){6,12}")
    if not p.match(field.data):
        raise ValidationError("Password must contain 6-12 characters including at least: 1 digit, 1 lowercase word "
                              "character, 1 uppercase word character, 1 special character")


class RegisterForm(FlaskForm):
    email = StringField(validators=[DataRequired(), Email()])
    firstname = StringField(validators=[DataRequired(), character_check])
    lastname = StringField(validators=[DataRequired(), character_check])
    phone = StringField(validators=[DataRequired(), phone_format])
    password = PasswordField(validators=[DataRequired(), password_format])
    confirm_password = PasswordField(validators=[DataRequired(), EqualTo('password', message="Passwords must be equal")])
    submit = SubmitField()


# Login
class LoginForm(FlaskForm):
    username = StringField(validators=[DataRequired()])
    password = PasswordField(validators=[DataRequired()])
    recaptcha = RecaptchaField(validators=[Recaptcha(message="Please confirm that you are human")])
    pin = StringField(validators=[DataRequired()])
    submit = SubmitField()
