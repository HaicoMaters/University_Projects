# IMPORTS
import logging
from datetime import datetime
import bcrypt
import pyotp
from flask import Blueprint, render_template, flash, redirect, url_for, session, request
from markupsafe import Markup
from flask_login import login_user, logout_user, current_user, login_required
from app import db, requires_roles, logout_required
from models import User
from users.forms import RegisterForm, LoginForm

# CONFIG
users_blueprint = Blueprint('users', __name__, template_folder='templates')


# VIEWS
# view registration
@users_blueprint.route('/register', methods=['GET', 'POST'])
@logout_required()
def register():

    # create signup form object
    form = RegisterForm()

    # if request method is POST or form is valid
    if form.validate_on_submit():
        user = User.query.filter_by(email=form.email.data).first()
        # if this returns a user, then the email already exists in database

        # if email already exists redirect user back to signup page with error message so user can try again
        if user:
            flash('Email address already exists')
            return render_template('users/register.html', form=form)

        # create a new user with the form data
        new_user = User(email=form.email.data,
                        firstname=form.firstname.data,
                        lastname=form.lastname.data,
                        phone=form.phone.data,
                        password=form.password.data,
                        role='user')

        # add the new user to the database
        db.session.add(new_user)
        db.session.commit()

        # log user registration
        logging.warning('SECURITY - User registration [%s, %s]',
                        form.email.data,
                        request.remote_addr
                        )

        # sends user to login page
        return redirect(url_for('users.login'))
    # if request method is GET or form not valid re-render signup page
    return render_template('users/register.html', form=form)


# view user login
@users_blueprint.route('/login', methods=['GET', 'POST'])
@logout_required()
def login():

    form = LoginForm()

    # Initialise session
    if not session.get('authentication_attempts'):
        session['authentication_attempts'] = 0

    # Authenticate submitted data
    if form.validate_on_submit():
        user = User.query.filter_by(email=form.username.data).first()
        if not user or not bcrypt.checkpw(form.password.data.encode('utf-8'), user.password) \
                or not pyotp.TOTP(user.pinkey).verify(form.pin.data):

            # log invalid login attempt
            logging.warning('SECURITY - Invalid Login Attempt [%s, %s]',
                            form.username.data,
                            request.remote_addr
                            )

            # Check login attempts
            session['authentication_attempts'] += 1
            if session.get('authentication_attempts') >= 3:
                flash(Markup('Number of incorrect login attempts exceeded. Please click <a href="/reset">here</a> to '
                             'reset.'))
                return render_template('users/login.html', form=form)
            flash('Please check your login details and try again you have ' + str(3 - session[
                'authentication_attempts']) + ' attempts left')
        else:
            login_user(user)

            # log user login
            logging.warning('SECURITY - Log in [%s, %s, %s]',
                            current_user.id,
                            current_user.email,
                            request.remote_addr
                            )

            # Record login
            user.last_login = user.current_login
            user.current_login = datetime.now()
            db.session.add(user)
            db.session.commit()

            # Redirect to appropriate url
            if current_user.role == "admin":
                return redirect(url_for('admin.admin'))
            else:
                return redirect(url_for('users.profile'))

    return render_template('users/login.html', form=form)


# Reset login attempts
@users_blueprint.route('/reset')
@logout_required()
def reset():

    session['authentication_attempts'] = 0
    return redirect(url_for('users.login'))


# Logout user
@users_blueprint.route('/logout')
@login_required
@requires_roles('user', 'admin')
def logout():
    # log user logging out
    logging.warning('SECURITY - Log out [%s, %s, %s]',
                    current_user.id,
                    current_user.email,
                    request.remote_addr
                    )

    logout_user()
    return redirect(url_for('index'))


# view user profile
@users_blueprint.route('/profile')
@login_required
@requires_roles('user')
def profile():
    return render_template('users/profile.html', name=current_user.firstname)


# view user account
@users_blueprint.route('/account')
@login_required
@requires_roles('user', 'admin')
def account():
    return render_template('users/account.html',
                           acc_no=current_user.id,
                           email=current_user.email,
                           firstname=current_user.firstname,
                           lastname=current_user.lastname,
                           phone=current_user.phone)
